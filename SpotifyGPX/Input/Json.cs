// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Input;

public partial class Json : SongInputBase
{
    private JsonNetDeserializer JsonDeserializer { get; }
    private List<JsonDocument> AllEntries { get; }
    protected override List<ISongEntry> AllSongs { get; }

    public Json(string path)
    {
        JsonDeserializer = new JsonNetDeserializer(path);
        AllEntries = JsonDeserializer.Deserialize<JsonDocument>();
        AllSongs = ParseSongs();
    }

    private List<ISongEntry> ParseSongs()
    {
        return AllEntries.Select((entry, index) =>
        {
            JsonElement root = entry.RootElement;

            string? time1 = root.TryGetProperty("endTime", out JsonElement smallTime) ? smallTime.GetString() : null;
            string? time2 = root.TryGetProperty("ts", out JsonElement largeTime) ? largeTime.GetString() : null;
            string time = time1 ?? time2 ?? throw new Exception($"Song timestamp missing from JSON entry {index}");

            string? username = root.TryGetProperty("username", out JsonElement un) ? un.GetString() : null;
            string? platform = root.TryGetProperty("platform", out JsonElement plat) ? plat.GetString() : null;

            int? duration1 = root.TryGetProperty("msPlayed", out JsonElement smallDuration) ? smallTime.GetInt32() : null;
            int? duration2 = root.TryGetProperty("ms_played", out JsonElement largeDuration) ? largeDuration.GetInt32() : null;
            int duration = duration1 ?? duration2 ?? throw new Exception($"Song duration missing from JSON entry {index}");

            string? country = root.TryGetProperty("conn_country", out JsonElement coun) ? coun.GetString() : null;
            string? ipAddress = root.TryGetProperty("ip_addr_decrypted", out JsonElement ipAddr) ? ipAddr.GetString() : null;
            string? userAgent = root.TryGetProperty("user_agent_decrypted", out JsonElement ua) ? ua.GetString() : null;

            string? name1 = root.TryGetProperty("trackName", out JsonElement smallName) ? smallName.GetString() : null;
            string? name2 = root.TryGetProperty("master_metadata_track_name", out JsonElement largeName) ? largeName.GetString() : null;
            string? name = name1 ?? name2;

            string? artist1 = root.TryGetProperty("artistName", out JsonElement smallArtist) ? smallArtist.GetString() : null;
            string? artist2 = root.TryGetProperty("master_metadata_album_artist_name", out JsonElement largeArtist) ? largeArtist.GetString() : null;
            string? artist = artist1 ?? artist2;

            string? album = root.TryGetProperty("master_metadata_album_album_name", out JsonElement al) ? al.GetString() : null;
            string? uri = root.TryGetProperty("spotify_track_uri", out JsonElement ur) ? ur.GetString() : null;
            string? episodeName = root.TryGetProperty("episode_name", out JsonElement episode) ? episode.GetString() : null;
            string? episodeShow = root.TryGetProperty("episode_show_name", out JsonElement show) ? show.GetString() : null;
            string? episodeUri = root.TryGetProperty("spotify_episode_uri", out JsonElement epUri) ? epUri.GetString() : null;
            string? startReason = root.TryGetProperty("reason_start", out JsonElement sReason) ? sReason.GetString() : null;
            string? endReason = root.TryGetProperty("reason_end", out JsonElement eReason) ? eReason.GetString() : null;
            bool? shuffled = root.TryGetProperty("shuffle", out JsonElement shuffle) ? shuffle.GetBoolean() : null;
            bool? skipped = root.TryGetProperty("skipped", out JsonElement skip) ? skip.GetBoolean() : null;
            bool? offline = root.TryGetProperty("offline", out JsonElement off) ? off.GetBoolean() : null;
            long? offlineTimestamp = root.TryGetProperty("offline_timestamp", out JsonElement offTs) ? offTs.GetInt64() : null;
            bool? incognito = root.TryGetProperty("incognito", out JsonElement incog) ? incog.GetBoolean() : null;

            return (ISongEntry)new SpotifyEntry
            {
                Index = index,
                CurrentInterpretation = Interpretation,
                FriendlyTime = DateTimeOffset.Parse(time),
                Spotify_Username = username,
                Spotify_Platform = platform,
                Time_Played = duration,
                Spotify_Country = country,
                Spotify_IP = ipAddress,
                Spotify_UA = userAgent,
                Song_Name = name,
                Song_Artist = artist,
                Song_Album = album,
                Song_URI = uri,
                Episode_Name = episodeName,
                Episode_Show = episodeShow,
                Episode_URI = episodeUri,
                Song_StartReason = startReason,
                Song_EndReason = endReason,
                Song_Shuffle = shuffled,
                Song_Skipped = skipped,
                Spotify_Offline = offline,
                Spotify_OfflineTS = offlineTimestamp,
                Spotify_Incognito = incognito
            };
        }).ToList();
    }

    public List<SpotifyEntry> FilterSongs(List<SpotifyEntry> songs)
    {
        return songs
            .Where(song => !(song.TimePlayed >= MinimumPlaytime) || !ExcludeSkipped || song.Song_Skipped != true).ToList();
    }

    public override int SourceSongCount => AllEntries.Count;
}
