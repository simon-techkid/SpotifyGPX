// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing song playback data from the JSON format.
/// </summary>
public partial class Json : ISongInput, IJsonDeserializer
{
    private JsonDeserializer JsonDeserializer { get; }
    private List<JObject> AllEntries { get; }
    private List<SpotifyEntry> AllSongs { get; } // All songs parsed from the JSON

    /// <summary>
    /// Creates a new input handler for handling files in the JSON format.
    /// </summary>
    /// <param name="path">The path of the JSON file.</param>
    public Json(string path)
    {
        JsonDeserializer = new JsonDeserializer(path, JsonSettings);
        AllEntries = Deserialize();
        AllSongs = ParseEntriesToSongs();
    }

    public List<JObject> Deserialize()
    {
        return JsonDeserializer.Deserialize();
    }

    public List<SpotifyEntry> ParseEntriesToSongs()
    {
        return AllEntries
            .Select((entry, index) => new SpotifyEntry(
                index,
                (DateTimeOffset?)entry["endTime"] ?? (DateTimeOffset?)entry["ts"] ?? throw new Exception(""),
                (string?)entry["username"],
                (string?)entry["platform"],
                (double?)entry["msPlayed"] ?? (double?)entry["ms_played"] ?? throw new Exception(""),
                (string?)entry["conn_country"],
                (string?)entry["ip_addr_decrypted"],
                (string?)entry["user_agent_decrypted"],
                (string?)entry["trackName"] ?? (string?)entry["master_metadata_track_name"],
                (string?)entry["artistName"] ?? (string?)entry["master_metadata_album_artist_name"],
                (string?)entry["master_metadata_album_album_name"],
                (string?)entry["spotify_track_uri"],
                (string?)entry["episode_name"],
                (string?)entry["episode_show_name"],
                (string?)entry["spotify_episode_uri"],
                (string?)entry["reason_start"],
                (string?)entry["reason_end"],
                (bool?)entry["shuffle"],
                (bool?)entry["skipped"],
                (bool?)entry["offline"],
                (long?)entry["offline_timestamp"],
                (bool?)entry["incognito"]
                )
            ).ToList();
    }

    /// <summary>
    /// Gets all the songs, as a list, from the JSON file.
    /// </summary>
    /// <returns>A list of all the SpotifyEntries in the JSON.</returns>
    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    /// <summary>
    /// The total number of songs contained in the JSON file.
    /// </summary>
    public int SongCount => AllSongs.Count;
}
