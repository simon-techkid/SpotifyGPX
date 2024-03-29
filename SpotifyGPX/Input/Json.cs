// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public partial class Json : SongInputBase
{
    private JsonDeserializer JsonDeserializer { get; }
    private List<JObject> AllEntries { get; }
    protected override List<SpotifyEntry> AllSongs { get; }

    public Json(string path)
    {
        JsonDeserializer = new JsonDeserializer(path, JsonSettings);
        AllEntries = JsonDeserializer.Deserialize();
        AllSongs = ParseSongs();
    }

    private List<SpotifyEntry> ParseSongs()
    {
        return AllEntries.Select((entry, index) => new SpotifyEntry
        {
            Index = index,
            TimeEnded = (DateTimeOffset?)entry["endTime"] ?? (DateTimeOffset?)entry["ts"] ?? throw new Exception($"'ts' timestamp missing from JSON entry {index}"),
            Spotify_Username = (string?)entry["username"],
            Spotify_Platform = (string?)entry["platform"],
            Time_Played = (int?)entry["msPlayed"] ?? (int?)entry["ms_played"] ?? throw new Exception($"'msPlayed' duration missing from JSON entry {index}"),
            Spotify_Country = (string?)entry["conn_country"],
            Spotify_IP = (string?)entry["ip_addr_decrypted"],
            Spotify_UA = (string?)entry["user_agent_decrypted"],
            Song_Name = (string?)entry["trackName"] ?? (string?)entry["master_metadata_track_name"],
            Song_Artist = (string?)entry["artistName"] ?? (string?)entry["master_metadata_album_artist_name"],
            Song_Album = (string?)entry["master_metadata_album_album_name"],
            Song_URI = (string?)entry["spotify_track_uri"],
            Episode_Name = (string?)entry["episode_name"],
            Episode_Show = (string?)entry["episode_show_name"],
            Episode_URI = (string?)entry["spotify_episode_uri"],
            Song_StartReason = (string?)entry["reason_start"],
            Song_EndReason = (string?)entry["reason_end"],
            Song_Shuffle = (bool?)entry["shuffle"],
            Song_Skipped = (bool?)entry["skipped"],
            Spotify_Offline = (bool?)entry["offline"],
            Spotify_OfflineTS = (long?)entry["offline_timestamp"],
            Spotify_Incognito = (bool?)entry["incognito"]
        }).ToList();
    }

    public override int SourceSongCount => AllEntries.Count;
}
