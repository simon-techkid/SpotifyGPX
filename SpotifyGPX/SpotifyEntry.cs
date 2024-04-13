// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using SpotifyGPX.Api;
using SpotifyGPX.Input;
using System;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// A record of a Spotify song played by the user. Contains metadata about the song itself as well as the time it was played.
/// </summary>
public partial struct SpotifyEntry
{
    private DateTimeOffset? _timeEnded; // if null, estimate end time. Not null if time is known.
    private DateTimeOffset? _timeStarted; // if null, estimate start time. Not null if time is known.

    /// <summary>
    /// The index of this song in a list of songs.
    /// </summary>
    [JsonProperty("SGPX_Index")]
    public int Index { get; set; }

    /// <summary>
    /// The official time of the song.
    /// This time will be used for pairing it with a <see cref="GPXPoint"/>. 
    /// If <see cref="TimeUsage"/> is <see cref="TimeUsage.Start"/>, use <see cref="TimeStarted"/>.
    /// If <see cref="TimeUsage"/> is <see cref="TimeUsage.End"/>, use <see cref="TimeEnded"/>
    /// </summary>
    [JsonProperty("SGPX_Time")]
    public DateTimeOffset Time
    {
        get
        {
            return TimeUsage switch
            {
                TimeUsage.Start => TimeStarted,
                TimeUsage.End => TimeEnded,
                _ => throw new InvalidOperationException("SpotifyEntry time usage not set.")
            };
        }
        set
        {
            switch (TimeInterpretation)
            {
                case TimeInterpretation.Start:
                    _timeStarted = value;
                    break;
                case TimeInterpretation.End:
                    _timeEnded = value;
                    break;
                default:
                    throw new InvalidOperationException("SpotifyEntry time interpretation not set.");
            }
        }
    }

    /// <summary>
    /// Determines whether to treat the given time (from an <see cref="ISongInput"/>) as the start or the end time of the <see cref="SpotifyEntry"/>.
    /// </summary>
    [JsonProperty("SGPX_TimeInterpretation")]
    public TimeInterpretation TimeInterpretation { get; set; }

    /// <summary>
    /// Determines whether the start or end time of the song should be used for the official <see cref="Time"/> time.
    /// </summary>
    [JsonProperty("SGPX_TimeUsage")]
    public readonly TimeUsage TimeUsage => timeUsage;

    /// <summary>
    /// The time and date this song ended.
    /// </summary>
    [JsonProperty("SGPX_TimeStarted")]
    public DateTimeOffset TimeStarted
    {
        get
        {
            if (!_timeStarted.HasValue)
                _timeStarted = TimeEnded - TimePlayed;

            if (_timeStarted != null)
                return _timeStarted.Value;

            throw new InvalidOperationException("SpotifyEntry time started not set.");
        }
    }

    [JsonProperty("ts")]
    public DateTimeOffset TimeEnded
    {
        get
        {
            if (!_timeEnded.HasValue)
                _timeEnded = TimeStarted + TimePlayed;

            if (_timeEnded != null)
                return _timeEnded.Value;

            throw new InvalidOperationException("SpotifyEntry time ended not set.");
        }
    }

    /// <summary>
    /// Whether or not <see cref="TimeStarted"/> is estimated based on the song duration.
    /// </summary>
    [JsonProperty("SGPX_TimeStartEstimated")]
    public readonly bool TimeStartEstimated => _timeStarted == null;

    /// <summary>
    /// Whether or not <see cref="TimeEnded"/> is estimated based on the song duration.
    /// </summary>
    [JsonProperty("SGPX_TimeEndEstimated")]
    public readonly bool TimeEndEstimated => _timeEnded == null;

    /// <summary>
    /// This field is your Spotify username.
    /// </summary>
    [JsonProperty("username")]
    public string? Spotify_Username { get; set; }

    /// <summary>
    /// This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
    /// </summary>
    [JsonProperty("platform")]
    public string? Spotify_Platform { get; set; }

    /// <summary>
    /// This field is the number of milliseconds the stream was played.
    /// </summary>
    [JsonProperty("msPlayed")]
    public int? Time_Played
    {
        readonly get => (int?)TimePlayed?.TotalMilliseconds ?? null;
        set => TimePlayed = value == null ? null : TimePlayed = TimeSpan.FromMilliseconds((double)value);
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("SGPX_TimePlayed")]
    public TimeSpan? TimePlayed { get; private set; }

    /// <summary>
    /// This field is the country code of the country where the stream was played (e.g. SE - Sweden).
    /// </summary>
    [JsonProperty("conn_country")]
    public string? Spotify_Country { get; set; }

    /// <summary>
    /// This field contains the IP address logged when streaming the track.
    /// </summary>
    [JsonProperty("ip_addr_decrypted")]
    public string? Spotify_IP { get; set; }

    /// <summary>
    /// This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari)
    /// </summary>
    [JsonProperty("user_agent_decrypted")]
    public string? Spotify_UA { get; set; }

    /// <summary>
    /// This field is the name of the track.
    /// </summary>
    [JsonProperty("master_metadata_track_name")]
    public string? Song_Name { get; set; }

    /// <summary>
    /// This field is the name of the artist, band or podcast.
    /// </summary>
    [JsonProperty("master_metadata_album_artist_name")]
    public string? Song_Artist { get; set; }

    /// <summary>
    /// This field is the name of the album of the track.
    /// </summary>
    [JsonProperty("master_metadata_album_album_name")]
    public string? Song_Album { get; set; }

    /// <summary>
    /// A Spotify URI, uniquely identifying the track in the form of “spotify:track:base-62 string”
    /// </summary>
    [JsonProperty("spotify_track_uri")]
    public string? Song_URI { get; set; }

    /// <summary>
    /// The Spotify URI (song ID) of this song.
    /// </summary>
    [JsonProperty("SGPX_Song_ID")]
    public readonly string? Song_ID => Song_URI?.Split(':').Last();

    /// <summary>
    /// The URL of this song.
    /// </summary>
    [JsonProperty("SGPX_Song_URL")]
    public readonly string? Song_URL => Song_ID == null ? null : $"http://open.spotify.com/track/{Song_ID}";

    /// <summary>
    /// This field contains the name of the episode of the podcast.
    /// </summary>
    [JsonProperty("episode_name")]
    public string? Episode_Name { get; set; }

    /// <summary>
    /// This field contains the name of the show of the podcast.
    /// </summary>
    [JsonProperty("episode_show_name")]
    public string? Episode_Show { get; set; }

    /// <summary>
    /// A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:base-62 string”
    /// </summary>
    [JsonProperty("spotify_episode_uri")]
    public string? Episode_URI { get; set; }

    /// <summary>
    /// This field is a value telling why the track started (e.g. “trackdone”).
    /// </summary>
    [JsonProperty("reason_start")]
    public string? Song_StartReason { get; set; }

    /// <summary>
    /// This field is a value telling why the track ended (e.g. “endplay”).
    /// </summary>
    [JsonProperty("reason_end")]
    public string? Song_EndReason { get; set; }

    /// <summary>
    /// This field has the value True or False depending on if shuffle mode was used when playing the track.
    /// </summary>
    [JsonProperty("shuffle")]
    public bool? Song_Shuffle { get; set; }

    /// <summary>
    /// This field indicates if the user skipped to the next song.
    /// </summary>
    [JsonProperty("skipped")]
    public bool? Song_Skipped { get; set; }

    /// <summary>
    /// This field indicates whether the track was played in offline mode (“True”) or not (“False”).
    /// </summary>
    [JsonProperty("offline")]
    public bool? Spotify_Offline { get; set; }

    /// <summary>
    /// This field is a timestamp of when offline mode was used, if used.
    /// </summary>
    [JsonProperty("offline_timestamp")]
    public long? Spotify_OfflineTS
    {
        readonly get => OfflineTimestamp?.ToUnixTimeSeconds();
        set => OfflineTimestamp = value == null ? null : DateTimeOffset.FromUnixTimeSeconds((long)value);
    }

    /// <summary>
    /// This field is a timestamp of when offline mode was used, if used.
    /// </summary>
    [JsonProperty("SGPX_OfflineTimestamp")]
    public DateTimeOffset? OfflineTimestamp { get; private set; }

    /// <summary>
    /// This field indicates whether the track was played in incognito mode (“True”) or not (“False”).
    /// </summary>
    [JsonProperty("incognito_mode")]
    public bool? Spotify_Incognito { get; set; }

    /// <summary>
    /// Optional Spotify API metadata layer for this track.
    /// </summary>
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
