// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Input;

public sealed partial class Json : SongInputBase
{
    private List<JsonDocument> AllEntries { get; }
    public override ISongInput.ParseSongsDelegate ParseSongsMethod => ParseSongs;
    public override ISongInput.FilterSongsDelegate FilterSongsMethod => FilterSongs;

    public Json(string path) : base(path)
    {
        using JsonNetDeserializer deserializer = new(path);
        AllEntries = deserializer.Deserialize<JsonDocument>(JsonOptions);
    }

    private List<ISongEntry> ParseSongs()
    {
        return AllEntries.Select((entry, index) =>
        {
            JsonElement root = entry.RootElement;

            string? time1 = JsonTools.TryGetProperty("endTime", root)?.GetString();
            string? time2 = JsonTools.TryGetProperty("ts", root)?.GetString();
            string time = time1 ?? time2 ?? throw new Exception($"Song timestamp missing from JSON entry {index}");
            DateTimeOffset dto = DateTimeOffset.ParseExact(time, SpotifyTimeFormat, null, TimeStyle);

            string? username = JsonTools.TryGetProperty("username", root)?.GetString();
            string? platform = JsonTools.TryGetProperty("platform", root)?.GetString();

            int? duration1 = JsonTools.TryGetProperty("msPlayed", root)?.GetInt32();
            int? duration2 = JsonTools.TryGetProperty("ms_played", root)?.GetInt32();
            int duration = duration1 ?? duration2 ?? throw new Exception($"Song duration missing from JSON entry {index}");

            string? country = JsonTools.TryGetProperty("conn_country", root)?.GetString();
            string? ipAddress = JsonTools.TryGetProperty("ip_addr_decrypted", root)?.GetString();
            string? userAgent = JsonTools.TryGetProperty("user_agent_decrypted", root)?.GetString();

            string? name1 = JsonTools.TryGetProperty("trackName", root)?.GetString();
            string? name2 = JsonTools.TryGetProperty("master_metadata_track_name", root)?.GetString();
            string? name = name1 ?? name2;

            string? artist1 = JsonTools.TryGetProperty("artistName", root)?.GetString();
            string? artist2 = JsonTools.TryGetProperty("master_metadata_album_artist_name", root)?.GetString();
            string? artist = artist1 ?? artist2;

            string? album = JsonTools.TryGetProperty("master_metadata_album_album_name", root)?.GetString();
            string? uri = JsonTools.TryGetProperty("spotify_track_uri", root)?.GetString();
            string? episodeName = JsonTools.TryGetProperty("episode_name", root)?.GetString();
            string? episodeShow = JsonTools.TryGetProperty("episode_show_name", root)?.GetString();
            string? episodeUri = JsonTools.TryGetProperty("spotify_episode_uri", root)?.GetString();
            string? startReason = JsonTools.TryGetProperty("reason_start", root)?.GetString();
            string? endReason = JsonTools.TryGetProperty("reason_end", root)?.GetString();
            bool? shuffle = JsonTools.TryGetProperty("shuffle", root)?.GetBoolean();
            bool? skipped = JsonTools.TryGetProperty("skipped", root)?.GetBoolean();
            bool? offline = JsonTools.TryGetProperty("offline", root)?.GetBoolean();
            long? offlineTimestamp = JsonTools.TryGetProperty("offline_timestamp", root)?.GetInt64();
            bool? incognito = JsonTools.TryGetProperty("incognito_mode", root)?.GetBoolean();

            string? time11 = JsonTools.TryGetProperty("endTime", root)?.GetString();
            string? time22 = JsonTools.TryGetProperty("ts", root)?.GetString();
            string timee = time11 ?? time22 ?? throw new Exception($"Song timestamp missing from JSON entry {index}");
            DateTimeOffset dtoo = DateTimeOffset.ParseExact(timee, SpotifyTimeFormat, null, TimeStyle);

            return (ISongEntry)new SpotifyEntry()
            {
                Index = index,
                CurrentInterpretation = Interpretation,
                FriendlyTime = dto,
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
                Song_Shuffle = shuffle,
                Song_Skipped = skipped,
                Spotify_Offline = offline,
                Spotify_OfflineTS = offlineTimestamp,
                Spotify_Incognito = incognito
            };
        }).ToList()!;
    }

    private List<ISongEntry> FilterSongs()
    {
        return AllSongs.OfType<SpotifyEntry>().Where(song => filter(song)).Select(song => (ISongEntry)song).ToList();
    }

    protected override void DisposeDocument()
    {
        AllEntries.ForEach(entry => entry.Dispose());
    }

    public override int SourceSongCount => AllEntries.Count;
}
