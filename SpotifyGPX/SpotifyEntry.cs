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
    public SpotifyEntry(int index, DateTimeOffset ts, string? username, string? platform, double msPlayed, string? conn_country, string? ip_addr_decrypted, string? user_agent_decrypted, string? master_metadata_track_name, string? master_metadata_album_artist_name, string? master_metadata_album_album_name, string? spotify_track_uri, string? episode_name, string? episode_show_name, string? spotify_episode_uri, string? reason_start, string? reason_end, bool? shuffle, bool? skipped, bool? offline, long? offline_timestamp, bool? incognito_mode)
    {
        Index = index;
        TimeEnded = UseEstStartTime ? ts + TimeSpan.FromMilliseconds(msPlayed) : ts; // must fix later
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

    /// <summary>
    /// Unique identifier of this SpotifyEntry in a list
    /// </summary>
    public readonly int Index { get; }

    /// <summary>
    /// The time the song started (<see cref="TimeStartedEst"/>) or ended (<see cref="TimeEnded"/>).
    /// </summary>
    [JsonProperty("ts")]
    public readonly DateTimeOffset Time => UseEstStartTime ? TimeStartedEst : TimeEnded;

    /// <summary>
    /// Determines whether or not to use <see cref="TimeStartedEst"/> as the reference time.
    /// </summary>
    public readonly bool UseEstStartTime => PreferEstimatedStartTime;

    /// <summary>
    /// This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time).
    /// </summary>
    public readonly DateTimeOffset TimeEnded { get; }

    /// <summary>
    /// The estimated time and date when the song started.
    /// Can be used in place of <see cref="TimeEnded"/> (provided by Spotify) if you prefer the pairings be based on when the song began.
    /// </summary>
    public readonly DateTimeOffset TimeStartedEst => TimeEnded - TimePlayed;

    /// <summary>
    /// This field is your Spotify username.
    /// </summary>
    [JsonProperty("username")]
    public readonly string? Spotify_Username { get; }

    /// <summary>
    /// This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
    /// </summary>
    [JsonProperty("platform")]
    public readonly string? Spotify_Platform { get; }

    /// <summary>
    /// This field is the number of milliseconds the stream was played.
    /// </summary>
    [JsonProperty("msPlayed")]
    private readonly double Time_Played { get; }

    /// <summary>
    /// The duration of playback of this song, parsed from <see cref="Time_Played"/>.
    /// </summary>
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(Time_Played);

    /// <summary>
    /// This field is the country code of the country where the stream was played (e.g. SE - Sweden).
    /// </summary>
    [JsonProperty("conn_country")]
    public readonly string? Spotify_Country { get; }

    /// <summary>
    /// This field contains the IP address logged when streaming the track.
    /// </summary>
    [JsonProperty("ip_addr_decrypted")]
    public readonly string? Spotify_IP { get; }

    /// <summary>
    /// This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari).
    /// </summary>
    [JsonProperty("user_agent_decrypted")]
    public readonly string? Spotify_UA { get; }

    /// <summary>
    /// This field is the name of the track.
    /// </summary>
    [JsonProperty("master_metadata_track_name")]
    public readonly string? Song_Name { get; }

    /// <summary>
    /// This field is the name of the artist, band or podcast.
    /// </summary>
    [JsonProperty("master_metadata_album_artist_name")]
    public readonly string? Song_Artist { get; }

    /// <summary>
    /// This field is the name of the album of the track.
    /// </summary>
    [JsonProperty("master_metadata_album_album_name")]
    public readonly string? Song_Album { get; }

    /// <summary>
    /// A Spotify URI, uniquely identifying the track in the form of spotify:track:base-62 string.
    /// </summary>
    [JsonProperty("spotify_track_uri")]
    public readonly string? Song_URI { get; }

    /// <summary>
    /// The base-62 identifier found at the end of the Spotify URI, parsed from <see cref="Song_URI"/>.
    /// </summary>
    public readonly string? Song_ID => Song_URI?.Split(':').Last();

    /// <summary>
    /// The URL leading to the song on Spotify, parsed from <see cref="Song_ID"/>.
    /// </summary>
    public readonly string? Song_URL => Song_ID == null ? null : $"http://open.spotify.com/track/{Song_ID}";

    /// <summary>
    /// This field contains the name of the episode of the podcast.
    /// </summary>
    [JsonProperty("episode_name")]
    public readonly string? Episode_Name { get; }

    /// <summary>
    /// This field contains the name of the show of the podcast.
    /// </summary>
    [JsonProperty("episode_show_name")]
    public readonly string? Episode_Show { get; }

    /// <summary>
    /// A Spotify Episode URI, uniquely identifying the podcast episode in the form of spotify:episode:base-62 string.
    /// </summary>
    [JsonProperty("spotify_episode_uri")]
    public readonly string? Episode_URI { get; }

    /// <summary>
    /// This field is a value telling why the track started (e.g. “trackdone”).
    /// </summary>
    [JsonProperty("reason_start")]
    public readonly string? Song_StartReason { get; }

    /// <summary>
    /// This field is a value telling why the track ended (e.g. “endplay”).
    /// </summary>
    [JsonProperty("reason_end")]
    public readonly string? Song_EndReason { get; }

    /// <summary>
    /// This field has the value True or False depending on if shuffle mode was used when playing the track.
    /// </summary>
    [JsonProperty("shuffle")]
    public readonly bool? Song_Shuffle { get; }

    /// <summary>
    /// This field indicates if the user skipped to the next song.
    /// </summary>
    [JsonProperty("skipped")]
    public readonly bool? Song_Skipped { get; }

    /// <summary>
    /// This field indicates whether the track was played in offline mode (“True”) or not (“False”).
    /// </summary>
    [JsonProperty("offline")]
    public readonly bool? Spotify_Offline { get; }

    /// <summary>
    /// This field is a timestamp of when offline mode was used, if used.
    /// </summary>
    [JsonProperty("offline_timestamp")]
    private readonly long? Spotify_OfflineTS { get; }

    /// <summary>
    /// The time and date of when offline mode was used, parsed from <see cref="Spotify_OfflineTS"/>.
    /// </summary>
    public readonly DateTimeOffset? OfflineTimestamp => Spotify_OfflineTS == null ? null : DateTimeOffset.FromUnixTimeSeconds((long)Spotify_OfflineTS);

    /// <summary>
    /// This field indicates whether the track was played in incognito mode (“True”) or not (“False”).
    /// </summary>
    [JsonProperty("incognito_mode")]
    public readonly bool? Spotify_Incognito { get; }

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
