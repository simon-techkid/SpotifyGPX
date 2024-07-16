// SpotifyGPX by Simon Field

using SpotifyAPI.Web;
using SpotifyGPX.Broadcasting;
using SpotifyGPX.SongEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyGPX.Api.SpotifyAPI;

public partial class SpotifyEntryMatcher :
    ApiMetadataHandler<SpotifyEntry, string, SpotifyApiEntry?, ISongEntry>
{
    protected override string BroadcasterPrefix => "SPOTIFY";
    protected override Func<string, bool> CustomFilter => id => !string.IsNullOrEmpty(id);
    protected override string NameOfEntity => "Spotify";
    protected override Func<SpotifyEntry, ISongEntry> Converter => entry => (ISongEntry)entry;

    public SpotifyEntryMatcher(List<ISongEntry> songs, StringBroadcaster bcaster) : base(songs, bcaster) { }

    private static SpotifyClient InitializeSpotifyClient()
    {
        if (CLIENT_ID == "YOUR_CLIENT_ID" || CLIENT_SECRET == "YOUR_CLIENT_SECRET")
            throw new Exception("Please set your Spotify API credentials in Api/Options.cs");

        // Initialize SpotifyClient with Client Credentials Flow
        SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
        ClientCredentialsRequest request = new(CLIENT_ID, CLIENT_SECRET);
        ClientCredentialsTokenResponse response = new OAuthClient(config).RequestToken(request).Result;
        string accessToken = response.AccessToken;
        return new SpotifyClient(accessToken);
    }

    protected override Dictionary<string, SpotifyApiEntry?> GetMetadatas(List<string> identifiers)
    {
        return GetAllEntries(identifiers);
    }

    /// <summary>
    /// Get a dictionary of Spotify API entries (<see cref="SpotifyApiEntry"/>) for an array of track IDs.
    /// </summary>
    /// <param name="trackIds">A string array where each element represents a track's ID. Can include duplicate IDs (they will be filtered for distinction later).</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> where TKey is a track ID, and TValue is the corresponding returned API metadata (<see cref="SpotifyApiEntry"/>) for that track.</returns>
    public Dictionary<string, SpotifyApiEntry?> GetAllEntries(List<string> trackIds)
    {
        SpotifyEntityHandler<FullTrack> spotifyEntityHandler = new TrackHandler(BCaster);
        Task<List<FullTrack>> tracksTask = spotifyEntityHandler.GetEntities(trackIds);
        List<FullTrack> tracks = tracksTask.Result;

        SpotifyEntityHandler<FullArtist> artistHandler = new ArtistHandler(BCaster);
        List<string> artistIds = tracks
            .SelectMany(track => track.Artists.Select(artist => artist.Id))
            .Distinct()
            .ToList();
        Task<List<FullArtist>> artistsTask = artistHandler.GetEntities(artistIds);
        List<FullArtist> artists = artistsTask.Result;

        return CreateApiEntries(tracks, artists);
    }

    private abstract class SpotifyEntityHandler<T> : StringBroadcasterBase
    {
        protected override string BroadcasterPrefix => "SPOTIFY ENTITY";

        protected readonly SpotifyClient _spotify;

        protected SpotifyEntityHandler(StringBroadcaster broadcaster) : base(broadcaster)
        {
            _spotify = InitializeSpotifyClient();
        }

        /// <summary>
        /// Get entities of type <typeparamref name="T"/> from the Spotify API.
        /// </summary>
        /// <param name="ids">A collection of Spotify IDs to request of the specified type <typeparamref name="T"/> of media.</param>
        /// <returns></returns>
        protected abstract Task<BaseResponse<T>> GetEntitiesFromApi(List<string> ids);

        /// <summary>
        /// Maximum batch size for requests of type <typeparamref name="T"/>.
        /// </summary>
        protected abstract int MaxBatchSize { get; }

        /// <summary>
        /// Name of entities to request of type <typeparamref name="T"/>.
        /// </summary>
        public abstract string NameOfEntity { get; }

        public async Task<List<T>> GetEntities(List<string> entityIds)
        {
            List<IEnumerable<string>> idChunks = SplitIntoChunks(entityIds, MaxBatchSize).ToList();
            int totalChunks = idChunks.Count;
            BCaster.Broadcast($"Created {totalChunks} request chunks of {MaxBatchSize} max {NameOfEntity} each");

            return (await Task.WhenAll(
                idChunks.Select(async (chunk, index) =>
                {
                    int chunkIndex = index + 1;
                    int entitiesInChunk = chunk.Count();

                    BCaster.Broadcast($"Chunk {chunkIndex} of {totalChunks}: requesting {entitiesInChunk} {NameOfEntity} from API...");
                    BaseResponse<T> response = await GetEntitiesFromApi(chunk.ToList());
                    BCaster.Broadcast($"{response.Items.Count}/{entitiesInChunk} response {NameOfEntity}");

                    return response.Items ?? new List<T>();
                })))
                .SelectMany(x => x)
                .ToList();
        }
    }

    private class BaseResponse<T>
    {
        public BaseResponse(List<T> items)
        {
            Items = items;
        }

        /// <summary>
        /// Collection of items of type <typeparamref name="T"/> (tracks, artists, albums, etc.)
        /// </summary>
        public List<T> Items { get; }
    }

    private class TrackHandler : SpotifyEntityHandler<FullTrack>
    {
        public TrackHandler(StringBroadcaster bcaster) : base(bcaster) { }

        protected override async Task<BaseResponse<FullTrack>> GetEntitiesFromApi(List<string> ids)
        {
            TracksRequest tracksRequest = new(ids);
            TracksResponse response = await _spotify.Tracks.GetSeveral(tracksRequest);
            return new BaseResponse<FullTrack>(response.Tracks);
        }

        protected override int MaxBatchSize => 50;
        public override string NameOfEntity => "tracks";
    }

    private class ArtistHandler : SpotifyEntityHandler<FullArtist>
    {
        public ArtistHandler(StringBroadcaster bcaster) : base(bcaster) { }

        protected override async Task<BaseResponse<FullArtist>> GetEntitiesFromApi(List<string> ids)
        {
            ArtistsRequest artistsRequest = new(ids);
            ArtistsResponse response = await _spotify.Artists.GetSeveral(artistsRequest);
            return new BaseResponse<FullArtist>(response.Artists);
        }

        protected override int MaxBatchSize => 50;
        public override string NameOfEntity => "artists";
    }

    private class AlbumHandler : SpotifyEntityHandler<FullAlbum>
    {
        public AlbumHandler(StringBroadcaster bcaster) : base(bcaster) { }

        protected override async Task<BaseResponse<FullAlbum>> GetEntitiesFromApi(List<string> ids)
        {
            AlbumsRequest albumsRequest = new(ids);
            AlbumsResponse response = await _spotify.Albums.GetSeveral(albumsRequest);
            return new BaseResponse<FullAlbum>(response.Albums);
        }

        protected override int MaxBatchSize => 20;
        public override string NameOfEntity => "albums";
    }

    private static Dictionary<string, SpotifyApiEntry?> CreateApiEntries(List<FullTrack> tracks, List<FullArtist> artists)
    {
        Dictionary<string, SpotifyApiEntry?> entries = new();

        foreach (FullTrack track in tracks)
        {
            // Get album info out of track info
            SimpleAlbum trackAlbum = track.Album;
            List<string> artistIds = track.Artists.Select(artist => artist.Id).ToList();
            List<FullArtist> trackArtists = artists.Where(artist => artistIds.Contains(artist.Id)).ToList();

            SpotifyApiEntry apiEntry = new()
            {
                Id = track.Id,
                Name = track.Name,
                Duration = track.DurationMs,
                Popularity = track.Popularity,
                Artists = trackArtists.Select(artist => new Artist
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Genres = artist.Genres.ToArray()
                }).ToList(),
                Album = new Album
                {
                    Id = trackAlbum.Id,
                    Name = trackAlbum.Name,
                    ReleaseDate = trackAlbum.ReleaseDate
                }
            };

            entries.Add(apiEntry.Id, apiEntry);
        }

        return entries;
    }
}
