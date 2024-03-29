// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using SpotifyGPX.Api;
using System;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// A record of a Spotify song played by the user. Contains metadata about the song itself as well as the time it was played.
/// </summary>
public partial struct SpotifyEntry
{
    [JsonProperty("SGPX_Index")]
    public int Index { get; set; }

    [JsonProperty("SGPX_Time")]
    public readonly DateTimeOffset Time => UseEstStartTime ? TimeStartedEst : TimeEnded;

    [JsonProperty("SGPX_UseEstStartTime")]
    public readonly bool UseEstStartTime => PreferEstimatedStartTime;

    [JsonProperty("ts")]
    public DateTimeOffset TimeEnded { get; set; }

    [JsonProperty("SGPX_TimeStartedEst")]
    public readonly DateTimeOffset TimeStartedEst => TimeEnded - TimePlayed;

    [JsonProperty("username")]
    public string? Spotify_Username { get; set; }

    [JsonProperty("platform")]
    public string? Spotify_Platform { get; set; }

    [JsonProperty("msPlayed")]
    public int Time_Played
    {
        readonly get => (int)TimePlayed.TotalMilliseconds;
        set => TimePlayed = TimeSpan.FromMilliseconds(value);
    }

    [JsonProperty("SGPX_PercentPlayed")]
    public readonly int PercentPlayed => (int)Math.Round((double)(Time_Played / Metadata?.Duration ?? Time_Played) * 100);

    [JsonProperty("SGPX_TimePlayed")]
    public TimeSpan TimePlayed { get; private set; }

    [JsonProperty("conn_country")]
    public string? Spotify_Country { get; set; }

    [JsonProperty("ip_addr_decrypted")]
    public string? Spotify_IP { get; set; }

    [JsonProperty("user_agent_decrypted")]
    public string? Spotify_UA { get; set; }

    [JsonProperty("master_metadata_track_name")]
    public string? Song_Name { get; set; }

    [JsonProperty("master_metadata_album_artist_name")]
    public string? Song_Artist { get; set; }

    [JsonProperty("master_metadata_album_album_name")]
    public string? Song_Album { get; set; }

    [JsonProperty("spotify_track_uri")]
    public string? Song_URI { get; set; }

    [JsonProperty("SGPX_Song_ID")]
    public readonly string? Song_ID => Song_URI?.Split(':').Last();

    [JsonProperty("SGPX_Song_URL")]
    public readonly string? Song_URL => Song_ID == null ? null : $"http://open.spotify.com/track/{Song_ID}";

    [JsonProperty("episode_name")]
    public string? Episode_Name { get; set; }

    [JsonProperty("episode_show_name")]
    public string? Episode_Show { get; set; }

    [JsonProperty("spotify_episode_uri")]
    public string? Episode_URI { get; set; }

    [JsonProperty("reason_start")]
    public string? Song_StartReason { get; set; }

    [JsonProperty("reason_end")]
    public string? Song_EndReason { get; set; }

    [JsonProperty("shuffle")]
    public bool? Song_Shuffle { get; set; }

    [JsonProperty("skipped")]
    public bool? Song_Skipped { get; set; }

    [JsonProperty("offline")]
    public bool? Spotify_Offline { get; set; }

    [JsonProperty("offline_timestamp")]
    public long? Spotify_OfflineTS { get; set; }

    [JsonProperty("SGPX_OfflineTimestamp")]
    public readonly DateTimeOffset? OfflineTimestamp => Spotify_OfflineTS == null ? null : DateTimeOffset.FromUnixTimeSeconds((long)Spotify_OfflineTS);

    [JsonProperty("incognito_mode")]
    public bool? Spotify_Incognito { get; set; }

    [JsonProperty("SGPX_Metadata")]
    public SpotifyApiEntry? Metadata { get; set; }

    /// <summary>
    /// Converts this SpotifyEntry to a string.
    /// </summary>
    /// <returns>The artist and name of this song, separated by a dash.</returns>
    public override readonly string ToString() => $"{Song_Artist} - {Song_Name}"; // Display format for this song

    /// <summary>
    /// Determines whether this song falls within a provided time frame.
    /// </summary>
    /// <param name="Start">The start of the time frame.</param>
    /// <param name="End">The end of the time frame.</param>
    /// <returns>True, if this song is within the provided time frame. False, if this song is outside the provided time frame.</returns>
    public bool WithinTimeFrame(DateTimeOffset Start, DateTimeOffset End) => (Time >= Start) && (Time <= End); // Return true if song within provided time range
}
