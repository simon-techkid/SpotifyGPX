// SpotifyGPX by Simon Field

using SpotifyGPX.Api;
using SpotifyGPX.Input;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SpotifyGPX;

public partial struct SpotifyEntry : ISongEntry, IEstimatableSong, ISpotifyApiCompat, ISpotifyApiProportionable, IUrlLinkableSong, IAlbumableSong
{
    public readonly override string ToString() => $"{Song_Artist} - {Song_Name}";

    [JsonIgnore]
    public string Description
    {
        get
        {
            StringBuilder builder = new();

            builder.AppendLine("Played for {0}", TimePlayed.ToString(Options.TimeSpan));
            builder.AppendLine("Skipped: {0}", Song_Skipped);
            builder.AppendLine("Shuffle: {0}", Song_Shuffle);
            builder.AppendLine("IP Address: {0}", Spotify_IP);
            builder.AppendLine("Listened: {0}%", PercentListened);
            builder.Append("Country: {0}", Spotify_Country);

            return builder.ToString();
        }
    }

    [JsonIgnore]
    public int Index { get; set; }

    [JsonPropertyName("ts")]
    public DateTimeOffset FriendlyTime { get; set; }

    [JsonIgnore]
    public TimeInterpretation CurrentInterpretation { get; set; }

    [JsonIgnore]
    public readonly TimeUsage CurrentUsage => timeUsage;

    /// <summary>
    /// This field is your Spotify username.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Spotify_Username { get; set; }

    /// <summary>
    /// This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
    /// </summary>
    [JsonPropertyName("platform")]
    public string? Spotify_Platform { get; set; }

    /// <summary>
    /// This field is the number of milliseconds the stream was played.
    /// </summary>
    [JsonPropertyName("ms_played")]
    public int Time_Played { get; set; }

    [JsonIgnore]
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(Time_Played);

    /// <summary>
    /// This field is the country code of the country where the stream was played (e.g. SE - Sweden).
    /// </summary>
    [JsonPropertyName("conn_country")]
    public string? Spotify_Country { get; set; }

    /// <summary>
    /// This field contains the IP address logged when streaming the track.
    /// </summary>
    [JsonPropertyName("ip_addr_decrypted")]
    public string? Spotify_IP { get; set; }

    /// <summary>
    /// This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari)
    /// </summary>
    [JsonPropertyName("user_agent_decrypted")]
    public string? Spotify_UA { get; set; }

    /// <summary>
    /// This field is the name of the track.
    /// </summary>
    [JsonPropertyName("master_metadata_track_name")]
    public string? Song_Name { get; set; }

    /// <summary>
    /// This field is the name of the artist, band or podcast.
    /// </summary>
    [JsonPropertyName("master_metadata_album_artist_name")]
    public string? Song_Artist { get; set; }

    /// <summary>
    /// This field is the name of the album of the track.
    /// </summary>
    [JsonPropertyName("master_metadata_album_album_name")]
    public string? Song_Album { get; set; }

    /// <summary>
    /// A Spotify URI, uniquely identifying the track in the form of “spotify:track:base-62 string”
    /// </summary>
    [JsonPropertyName("spotify_track_uri")]
    public string? Song_URI { get; set; }

    /// <summary>
    /// The Spotify URI (song ID) of this song.
    /// </summary>
    [JsonIgnore]
    public readonly string? Song_ID => Song_URI?.Split(':').Last();

    /// <summary>
    /// The URL of this song.
    /// </summary>
    [JsonIgnore]
    public readonly string? Song_URL => Song_ID == null ? null : $"http://open.spotify.com/track/{Song_ID}";

    /// <summary>
    /// This field contains the name of the episode of the podcast.
    /// </summary>
    [JsonPropertyName("episode_name")]
    public string? Episode_Name { get; set; }

    /// <summary>
    /// This field contains the name of the show of the podcast.
    /// </summary>
    [JsonPropertyName("episode_show_name")]
    public string? Episode_Show { get; set; }

    /// <summary>
    /// A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:base-62 string”
    /// </summary>
    [JsonPropertyName("spotify_episode_uri")]
    public string? Episode_URI { get; set; }

    /// <summary>
    /// This field is a value telling why the track started (e.g. “trackdone”).
    /// </summary>
    [JsonPropertyName("reason_start")]
    public string? Song_StartReason { get; set; }

    /// <summary>
    /// This field is a value telling why the track ended (e.g. “endplay”).
    /// </summary>
    [JsonPropertyName("reason_end")]
    public string? Song_EndReason { get; set; }

    /// <summary>
    /// This field has the value True or False depending on if shuffle mode was used when playing the track.
    /// </summary>
    [JsonPropertyName("shuffle")]
    public bool? Song_Shuffle { get; set; }

    /// <summary>
    /// This field indicates if the user skipped to the next song.
    /// </summary>
    [JsonPropertyName("skipped")]
    public bool? Song_Skipped { get; set; }

    /// <summary>
    /// This field indicates whether the track was played in offline mode (“True”) or not (“False”).
    /// </summary>
    [JsonPropertyName("offline")]
    public bool? Spotify_Offline { get; set; }

    /// <summary>
    /// This field is a timestamp of when offline mode was used, if used.
    /// </summary>
    [JsonPropertyName("offline_timestamp")]
    public long? Spotify_OfflineTS
    {
        readonly get => OfflineTimestamp?.ToUnixTimeSeconds();
        set => OfflineTimestamp = value == null ? null : DateTimeOffset.FromUnixTimeSeconds((long)value);
    }

    /// <summary>
    /// This field is a timestamp of when offline mode was used, if used.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset? OfflineTimestamp { get; private set; }

    /// <summary>
    /// This field indicates whether the track was played in incognito mode (“True”) or not (“False”).
    /// </summary>
    [JsonPropertyName("incognito")]
    public bool? Spotify_Incognito { get; set; }

    [JsonIgnore]
    public decimal? PercentListened => Metadata?.Duration != null ? Math.Round(((decimal)Time_Played / (decimal)Metadata?.Duration!) * 100, 0) : null;

    [JsonIgnore]
    public SpotifyApiEntry? Metadata { get; set; }
}
