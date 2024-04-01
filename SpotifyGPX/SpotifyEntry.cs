// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// A record of a Spotify song played by the user. Contains metadata about the song itself as well as the time it was played.
/// </summary>
public readonly partial struct SpotifyEntry
{
    [JsonConstructor]
    public SpotifyEntry(
        int index,
        DateTimeOffset ts, // Can be start or end time
        string? username,
        string? platform,
        int msPlayed,
        string? conn_country,
        string? ip_addr_decrypted,
        string? user_agent_decrypted,
        string? master_metadata_track_name,
        string? master_metadata_album_artist_name,
        string? master_metadata_album_album_name,
        string? spotify_track_uri,
        string? episode_name,
        string? episode_show_name,
        string? spotify_episode_uri,
        string? reason_start,
        string? reason_end,
        bool? shuffle,
        bool? skipped,
        bool? offline,
        long? offline_timestamp,
        bool? incognito_mode)
    {
        Index = index;
        TimeEnded = InterpretAsStartTime ? ts + TimeSpan.FromMilliseconds(msPlayed) : ts; // must fix later
        Spotify_Username = username;
        Spotify_Platform = platform;
        Time_Played = msPlayed;
        Spotify_Country = conn_country;
        Spotify_IP = ip_addr_decrypted;
        Spotify_UA = user_agent_decrypted;
        Song_Name = master_metadata_track_name;
        Song_Artist = master_metadata_album_artist_name;
        Song_Album = master_metadata_album_album_name;
        Song_URI = spotify_track_uri;
        Episode_Name = episode_name;
        Episode_Show = episode_show_name;
        Episode_URI = spotify_episode_uri;
        Song_StartReason = reason_start;
        Song_EndReason = reason_end;
        Song_Shuffle = shuffle;
        Song_Skipped = skipped;
        Spotify_Offline = offline;
        Spotify_OfflineTS = offline_timestamp;
        Spotify_Incognito = incognito_mode;
    }

    [JsonProperty("SGPX_Index")]
    public int Index { get; }

    [JsonProperty("SGPX_Time")]
    public DateTimeOffset Time => UseEstStartTime ? TimeStartedEst : TimeEnded;

    [JsonProperty("SGPX_UseEstStartTime")]
    public bool UseEstStartTime => PreferEstimatedStartTime;

    [JsonProperty("ts")]
    public DateTimeOffset TimeEnded { get; }

    [JsonProperty("SGPX_TimeStartedEst")]
    public DateTimeOffset TimeStartedEst => TimeEnded - TimePlayed;

    [JsonProperty("username")]
    public string? Spotify_Username { get; }

    [JsonProperty("platform")]
    public string? Spotify_Platform { get; }

    [JsonProperty("msPlayed")]
    public int Time_Played { get; }

    [JsonProperty("SGPX_TimePlayed")]
    public TimeSpan TimePlayed => TimeSpan.FromMilliseconds(Time_Played);

    [JsonProperty("conn_country")]
    public string? Spotify_Country { get; }

    [JsonProperty("ip_addr_decrypted")]
    public string? Spotify_IP { get; }

    [JsonProperty("user_agent_decrypted")]
    public string? Spotify_UA { get; }

    [JsonProperty("master_metadata_track_name")]
    public string? Song_Name { get; }

    [JsonProperty("master_metadata_album_artist_name")]
    public string? Song_Artist { get; }

    [JsonProperty("master_metadata_album_album_name")]
    public string? Song_Album { get; }

    [JsonProperty("spotify_track_uri")]
    public string? Song_URI { get; }

    [JsonProperty("SGPX_Song_ID")]
    public string? Song_ID => Song_URI?.Split(':').Last();

    [JsonProperty("SGPX_Song_URL")]
    public string? Song_URL => Song_ID == null ? null : $"http://open.spotify.com/track/{Song_ID}";

    [JsonProperty("episode_name")]
    public string? Episode_Name { get; }

    [JsonProperty("episode_show_name")]
    public string? Episode_Show { get; }

    [JsonProperty("spotify_episode_uri")]
    public string? Episode_URI { get; }

    [JsonProperty("reason_start")]
    public string? Song_StartReason { get; }

    [JsonProperty("reason_end")]
    public string? Song_EndReason { get; }

    [JsonProperty("shuffle")]
    public bool? Song_Shuffle { get; }

    [JsonProperty("skipped")]
    public bool? Song_Skipped { get; }

    [JsonProperty("offline")]
    public bool? Spotify_Offline { get; }

    [JsonProperty("offline_timestamp")]
    private long? Spotify_OfflineTS { get; }

    [JsonProperty("SGPX_OfflineTimestamp")]
    public DateTimeOffset? OfflineTimestamp => Spotify_OfflineTS == null ? null : DateTimeOffset.FromUnixTimeSeconds((long)Spotify_OfflineTS);

    [JsonProperty("incognito_mode")]
    public bool? Spotify_Incognito { get; }

    /// <summary>
    /// Converts this SpotifyEntry to a string.
    /// </summary>
    /// <returns>The artist and name of this song, separated by a dash.</returns>
    public override string ToString() => $"{Song_Artist} - {Song_Name}"; // Display format for this song

    /// <summary>
    /// Determines whether this song falls within a provided time frame.
    /// </summary>
    /// <param name="Start">The start of the time frame.</param>
    /// <param name="End">The end of the time frame.</param>
    /// <returns>True, if this song is within the provided time frame. False, if this song is outside the provided time frame.</returns>
    public bool WithinTimeFrame(DateTimeOffset Start, DateTimeOffset End) => (Time >= Start) && (Time <= End); // Return true if song within provided time range
}
