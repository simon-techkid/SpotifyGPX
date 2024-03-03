// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

/// <summary>
/// A journey track, with a name, type, and series of points representing the path on Earth of the journey. 
/// </summary>
public readonly struct GPXTrack
{
    /// <summary>
    /// Creates a GPXTrack, holding a series of points.
    /// </summary>
    /// <param name="index">The index of this track (in a series of tracks)</param>
    /// <param name="name">The friendly name of this track.</param>
    /// <param name="type">The type of this track (GPX, Gap, or Combined).</param>
    /// <param name="points">A list of the points comprising this track.</param>
    public GPXTrack(int? index, string? name, TrackType type, List<GPXPoint> points)
    {
        Track = new TrackInfo(index, name, type);
        Points = points;

        Start = Points.Select(point => point.Time).Min(); // Earliest point's time
        End = Points.Select(point => point.Time).Max(); // Latest point's time

        // Either above or below start/end parsing works, your choice

        // Start = Points.Select(point => point.Time).First(); // First point's time
        // End = Points.Select(point => point.Time).Last(); // Last point's time
    }

    /// <summary>
    /// Information about this track (such as its name, index in a list, and type).
    /// </summary>
    public readonly TrackInfo Track { get; } // Metadata for this track, including its name and index in a list

    /// <summary>
    /// A series of points that comprise this track (journey).
    /// </summary>
    public readonly List<GPXPoint> Points { get; } // Where and when were all the points in this track taken?

    /// <summary>
    /// The time and date at which this track's earliest point was taken.
    /// </summary>
    public readonly DateTimeOffset Start { get; } // What time was the earliest point logged?

    /// <summary>
    /// The time and date at which this track's latest point was taken.
    /// </summary>
    public readonly DateTimeOffset End { get; } // What time was the latest point logged?

    /// <summary>
    /// Converts this GPXTrack object to a string.
    /// </summary>
    /// <returns>A string, containing the name, number of points, start and end times, and type of the track.</returns>
    public override string ToString() // Display format for this track
    {
        StringBuilder builder = new();

        builder.Append("   Name: {0}", Track.ToString());
        builder.Append("   Points: {0}", Points.Count);
        builder.Append("   Starts: {0}", Start.ToString(Options.ISO8601Offset));
        builder.Append("   Ends: {0}", End.ToString(Options.ISO8601Offset));
        builder.Append("   Type: {0}", Track.Type);

        return builder.ToString();
    }
}

/// <summary>
/// Metadata for a journey track.
/// </summary>
public readonly struct TrackInfo
{
    /// <summary>
    /// Creates a TrackInfo object for holding track information.
    /// </summary>
    /// <param name="index">The index of this track (in a series of tracks).</param>
    /// <param name="name">The friendly name of this track.</param>
    /// <param name="type">The type of this track (GPX, Gap, or Combined).</param>
    [JsonConstructor]
    public TrackInfo(int? index, string? name, TrackType type)
    {
        Indexx = index;
        NodeName = name;
        Type = type;
    }

    /// <summary>
    /// The index of this track (as provided to the constructor).
    /// </summary>
    private readonly int? Indexx { get; }

    /// <summary>
    /// The index of this track in a series of tracks.
    /// If no index, a generic index (based on track type) will be used.
    /// </summary>
    public readonly int Index => Indexx == null ? (int)Type : (int)Indexx;

    /// <summary>
    /// The name of this track (as provided to the constructor).
    /// </summary>
    private readonly string? NodeName { get; }

    /// <summary>
    /// The friendly name of this track.
    /// If no name, a generic name will be used.
    /// </summary>
    public readonly string Name => NodeName ?? $"T{Index}";

    /// <summary>
    /// The type of track represented (GPX, Gap, or Combined).
    /// </summary>
    public TrackType Type { get; }

    /// <summary>
    /// Converts this TrackInfo object to a string.
    /// </summary>
    /// <returns>The name of the track.</returns>
    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is TrackInfo other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(TrackInfo other) => Index == other.Index && Name == other.Name && Type == other.Type;

    public static bool operator ==(TrackInfo left, TrackInfo right) => left.Equals(right);

    public static bool operator !=(TrackInfo left, TrackInfo right) => !(left == right);
}

/// <summary>
/// The list of possible types a journey track can represent.
/// </summary>
public enum TrackType
{
    /// <summary>
    /// A track created from original GPS data.
    /// </summary>
    GPX,

    /// <summary>
    /// A track created from gaps between GPS tracks.
    /// </summary>
    Gap,

    /// <summary>
    /// A single track containing all journey data combined (including gaps).
    /// </summary>
    Combined
}

/// <summary>
/// A single point holding geopositioning information, including coordinate and time, of one space in time.
/// </summary>
public readonly struct GPXPoint
{
    /// <summary>
    /// Creates a GPXPoint for holding a single point of geopositioning information.
    /// </summary>
    /// <param name="index">The index of this GPXPoint (in a created list).</param>
    /// <param name="point">The coordinate (pair) of this point's position.</param>
    /// <param name="time">The string representing the time of the point.</param>
    [JsonConstructor]
    public GPXPoint(int index, Coordinate location, DateTimeOffset time)
    {
        Index = index;
        Location = location;
        Time = time;
    }

    /// <summary>
    /// Creates a GPXPoint with a new coordinate, based on an existing GPXPoint. 
    /// </summary>
    /// <param name="oldPoint">An existing GPXPoint.</param>
    /// <param name="newCoord">The new coordinate for this GPXPoint</param>
    public GPXPoint(GPXPoint oldPoint, Coordinate newCoord) // Used for prediction only
    {
        this = oldPoint;
        Location = newCoord;
    }

    /// <summary>
    /// Unique identifier of this GPXPoint in a list.
    /// </summary>
    public readonly int Index { get; }

    /// <summary>
    /// The coordinates (lat/lon pair) of this point on Earth.
    /// </summary>
    public readonly Coordinate Location { get; }

    /// <summary>
    /// The time and date at which this point was taken.
    /// </summary>
    public readonly DateTimeOffset Time { get; }
}

/// <summary>
/// A latitude/longitude pair.
/// </summary>
public readonly struct Coordinate
{
    /// <summary>
    /// Creates a coordinate object with a latitude and longitude.
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lon">Longitude</param>
    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// This coordinate pair's latitude value.
    /// </summary>
    public readonly double Latitude { get; }

    /// <summary>
    /// This coordinate pair's longitude value.
    /// </summary>
    public readonly double Longitude { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not Coordinate)
        {
            return false;
        }

        Coordinate other = (Coordinate)obj;
        return Latitude == other.Latitude && Longitude == other.Longitude;
    }

    public static bool operator ==(Coordinate c1, Coordinate c2) => c1.Equals(c2);

    public static bool operator !=(Coordinate c1, Coordinate c2) => !c1.Equals(c2);

    /// <summary>
    /// Adds two coordinates together.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A coordinate representing the added coordinates.</returns>
    public static Coordinate operator +(Coordinate c1, Coordinate c2)
    {
        double latSum = c1.Latitude + c2.Latitude;
        double lonSum = c1.Longitude + c2.Longitude;
        return new Coordinate(latSum, lonSum);
    }

    /// <summary>
    /// Subtracts one coordinate from another.
    /// </summary>
    /// <param name="c1">The first coordinate</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A coordinate representing the difference between the coordinates.</returns>
    public static Coordinate operator -(Coordinate c1, Coordinate c2)
    {
        double latDiff = c1.Latitude - c2.Latitude;
        double lonDiff = c1.Longitude - c2.Longitude;
        return new Coordinate(latDiff, lonDiff);
    }

    /// <summary>
    /// Multiplies a coordinate by a scalar.
    /// </summary>
    /// <param name="c">A coordinate object.</param>
    /// <param name="scalar">The scalar value to shift the coordinate by.</param>
    /// <returns>A new coordinate representing the scaled original coordinate.</returns>
    public static Coordinate operator *(Coordinate c, double scalar)
    {
        double latScaled = c.Latitude * scalar;
        double lonScaled = c.Longitude * scalar;
        return new Coordinate(latScaled, lonScaled);
    }

    /// <summary>
    /// Calculates the distance between two coordinates.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A double representing the distance between the two coordinates.</returns>
    public static double CalculateDistance(Coordinate c1, Coordinate c2)
    {
        double latDiff = c2.Latitude - c1.Latitude;
        double lonDiff = c2.Longitude - c1.Longitude;

        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        return distance;
    }
}

/// <summary>
/// A Song-Point pair created from a correlation between a place and a played song.
/// </summary>
public readonly struct SongPoint
{
    /// <summary>
    /// The description of this pair, as printed to description fields.
    /// </summary>
    public readonly string Description
    {
        get
        {
            StringBuilder builder = new();

            string activity = Song.UseEstStartTime ? "started (est)" : "ended";

            builder.Append("At this position: {0}", PointTime.ToString(Options.ISO8601Offset));
            builder.Append("Song {0}", $"{activity}: {SongTime.ToString(Options.ISO8601Offset)}");
            builder.Append("Played for {0}", Song.TimePlayed.ToString(Options.TimeSpan));
            builder.Append("Skipped: {0}", Song.Song_Skipped);
            builder.Append("Shuffle: {0}", Song.Song_Shuffle);
            builder.Append("IP Address: {0}", Song.Spotify_IP);
            builder.Append("Country: {0}", Song.Spotify_Country);
            builder.Append("Predicted Index: {0}", PredictedIndex != null ? PredictedIndex : null);

            return builder.ToString();
        }
    }

    /// <summary>
    /// Create a new SongPoint pairing.
    /// </summary>
    /// <param name="index">The index of this SongPoint (in a created list).</param>
    /// <param name="song">The SpotifyEntry (containing song data) of this pair's song.</param>
    /// <param name="point">The GPXPoint (containing geospatial data) of this pair's point.</param>
    /// <param name="origin">The TrackInfo (track information) about the track from which this pair was created.</param>
    [JsonConstructor]
    public SongPoint(int index, SpotifyEntry song, GPXPoint point, TrackInfo origin)
    {
        Index = index;
        Song = song;
        Point = point;
        Origin = origin;
        PredictedIndex = null;
    }

    /// <summary>
    /// Creates a SongPoint pairing with a new Coordinate (lat/lon), based on an existing SongPoint pairing.
    /// </summary>
    /// <param name="oldPair">An existing SongPoint pairing.</param>
    /// <param name="newCoord">The new coordinate for this SongPoint.</param>
    /// <param name="relIndex">The index of this prediction in a set of predictions.</param>
    public SongPoint(SongPoint oldPair, Coordinate newCoord, int relIndex) // Used for prediction only
    {
        this = oldPair;
        Point = new GPXPoint(oldPair.Point, newCoord); // Create a GPXPoint using an existing point, with a new coordinate
        PredictedIndex = relIndex;
    }

    /// <summary>
    /// Unique identifier of this SongPoint in a list.
    /// </summary>
    public readonly int Index { get; }

    /// <summary>
    /// This song-point pair's song data.
    /// </summary>
    public readonly SpotifyEntry Song { get; }

    /// <summary>
    /// This song-point pair's point data.
    /// </summary>
    public readonly GPXPoint Point { get; }

    /// <summary>
    /// Information about the track from which the point was created.
    /// </summary>
    public readonly TrackInfo Origin { get; }

    /// <summary>
    /// The total number of seconds between the song and the point.
    /// </summary>
    public readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds;

    /// <summary>
    /// The absolute value (in seconds) between the song and the point.
    /// </summary>
    public readonly double AbsAccuracy => Math.Abs(Accuracy);

    /// <summary>
    /// The rounded number of seconds between the song and the point.
    /// </summary>
    private readonly double RoundAccuracy => Math.Round(Accuracy);

    /// <summary>
    /// This pair's UTC offset, defined by the offset of the point's time.
    /// </summary>
    public readonly TimeSpan NormalizedOffset => Point.Time.Offset;

    /// <summary>
    /// The time and date the song ended, converted to the pair's UTC offset (NormalizedOffset).
    /// </summary>
    public DateTimeOffset SongTime => Song.Time.ToOffset(NormalizedOffset);

    /// <summary>
    /// The time and date the point was taken, converted to the pair's UTC offset (NormalizedOffset).
    /// </summary>
    public DateTimeOffset PointTime => Point.Time.ToOffset(NormalizedOffset);

    /// <summary>
    /// The index of this pair in a series of predictions (if it's point was predicted).
    /// If not predicted, null.
    /// </summary>
    public int? PredictedIndex { get; }

    /// <summary>
    /// Converts this SongPoint pairing to a string.
    /// </summary>
    /// <returns>A single line (to be printed to the console), representing this pairing.</returns>
    public override string ToString()
    {
        // Set both the song and point times to the UTC offset provided by the original GPX point
        string songTime = SongTime.ToString(Options.TimeOnly);
        string pointTime = PointTime.ToString(Options.TimeOnly);

        // Print information about the pairing
        return $"[{Origin.ToString()}] [P{Point.Index}, S{Song.Index} ==> #{Index}] [{songTime}S ~ {pointTime}P] [{RoundAccuracy}s] {Song.ToString()}";
    }
}

/// <summary>
/// The StringBuilder used to add non-null objects as lines to a pair's description.
/// </summary>
public class StringBuilder
{
    private readonly System.Text.StringBuilder builder;

    public StringBuilder() => builder = new System.Text.StringBuilder();

    /// <summary>
    /// Appends the provided value to a string on a new line.
    /// </summary>
    /// <param name="format">The format of the given new line.</param>
    /// <param name="value">The value to be placed on the new line.</param>
    /// <returns>The given StringBuilder, with the new line added (if the provided value wasn't null).</returns>
    public StringBuilder Append(string format, object? value)
    {
        if (value != null)
        { // If appended value not null, append the line to the builder
            builder.AppendLine(string.Format(format, value));
        } // If null, the builder will be returned unchanged
        return this; // Return the builder
    }

    /// <summary>
    /// Converts this StringBuilder to a string.
    /// </summary>
    /// <returns>This StringBuilder, as a string.</returns>
    public override string ToString() => builder.ToString();
}