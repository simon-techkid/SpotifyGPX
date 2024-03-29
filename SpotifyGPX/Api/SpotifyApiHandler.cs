// SpotifyGPX by Simon Field

using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyGPX.Api;

/// <summary>
/// A class for handling Spotify API requests.
/// </summary>
public partial class SpotifyApiHandler
{
    private static SpotifyClient _spotify;
    private const int MaxSongsInBatch = 50;
    private const int MaxArtistsInBatch = 50;
    private const int MaxAlbumsInBatch = 20;

    static SpotifyApiHandler()
    {
        _spotify = InitializeSpotifyClient();
    }

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

    /// <summary>
    /// Get a dictionary of Spotify API entries (<see cref="SpotifyApiEntry"/>) for an array of track IDs.
    /// </summary>
    /// <param name="trackIds">A string array where each element represents a track's ID. Can include duplicate IDs (they will be filtered for distinction later).</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> where TKey is a track ID, and TValue is the corresponding returned API metadata (<see cref="SpotifyApiEntry"/>) for that track.</returns>
    public static Dictionary<string, SpotifyApiEntry> GetAllEntries(string[] trackIds)
    {
        SpotifyEntityHandler<FullTrack> spotifyEntityHandler = new TrackHandler();
        List<FullTrack> tracks = spotifyEntityHandler.GetEntities(trackIds, MaxSongsInBatch, "songs").Result;

        SpotifyEntityHandler<FullArtist> artistHandler = new ArtistHandler();
        List<FullArtist> artists = artistHandler.GetEntities(tracks.SelectMany(track => track.Artists.Select(artist => artist.Id)).ToArray(), MaxArtistsInBatch, "artists").Result;

        return CreateApiEntries(tracks, artists);
    }

    private abstract class SpotifyEntityHandler<T>
    {
        protected abstract Task<BaseResponse<T>> GetEntitiesFromApi(string[] ids);

        public async Task<List<T>> GetEntities(string[] entityIds, int maxBatchSize, string entityName)
        {
            List<string> distinctIds = entityIds.Distinct().ToList();
            Console.WriteLine($"[API] Filtered {distinctIds.Count} unique {entityName} IDs from {entityIds.Length} total {entityName} IDs");

            List<IEnumerable<string>> idChunks = SplitIntoChunks(distinctIds, maxBatchSize).ToList();
            int totalChunks = idChunks.Count;
            Console.WriteLine($"[API] Created {totalChunks} request chunks of {maxBatchSize} max {entityName.ToLower()} each");

            List<T> entities = new();
            foreach (IEnumerable<string> chunk in idChunks)
            {
                int chunkIndex = idChunks.IndexOf(chunk) + 1;
                int entitiesInChunk = chunk.Count();

                Console.Write($"[API] Chunk {chunkIndex} of {totalChunks}: requesting {entitiesInChunk} {entityName.ToLower()} from API...");
                BaseResponse<T> response = await GetEntitiesFromApi(chunk.ToArray());
                Console.WriteLine($"{response.Items.Count}/{entitiesInChunk} response {entityName.ToLower()}");

                if (response.Items != null)
                    entities.AddRange(response.Items);
            }

            return entities;
        }
    }

    private class BaseResponse<T>
    {
        public BaseResponse(List<T> items)
        {
            Items = items;
        }

        /// <summary>
        /// Collection of items of type T (tracks, artists, albums, etc.)
        /// </summary>
        public List<T> Items { get; }
    }

    private class TrackHandler : SpotifyEntityHandler<FullTrack>
    {
        protected override async Task<BaseResponse<FullTrack>> GetEntitiesFromApi(string[] ids)
        {
            TracksRequest tracksRequest = new(ids);
            TracksResponse response = await _spotify.Tracks.GetSeveral(tracksRequest);
            return new BaseResponse<FullTrack>(response.Tracks);
        }
    }

    private class ArtistHandler : SpotifyEntityHandler<FullArtist>
    {
        protected override async Task<BaseResponse<FullArtist>> GetEntitiesFromApi(string[] ids)
        {
            ArtistsRequest artistsRequest = new(ids);
            ArtistsResponse response = await _spotify.Artists.GetSeveral(artistsRequest);
            return new BaseResponse<FullArtist>(response.Artists);
        }
    }

    private class AlbumHandler : SpotifyEntityHandler<FullAlbum>
    {
        protected override async Task<BaseResponse<FullAlbum>> GetEntitiesFromApi(string[] ids)
        {
            AlbumsRequest albumsRequest = new(ids);
            AlbumsResponse response = await _spotify.Albums.GetSeveral(albumsRequest);
            return new BaseResponse<FullAlbum>(response.Albums);
        }
    }

    private static Dictionary<string, SpotifyApiEntry> CreateApiEntries(List<FullTrack> tracks, List<FullArtist> artists)
    {
        Dictionary<string, SpotifyApiEntry> entries = new();

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

    private static IEnumerable<IEnumerable<T>> SplitIntoChunks<T>(IEnumerable<T> source, int chunkSize)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }
}
