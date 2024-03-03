// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// A record of a Spotify song played by the user. Contains metadata about the song itself as well as the time it was played.
/// </summary>
public readonly partial struct SpotifyEntry
{
    /// <summary>
    /// Creates a SpotifyEntry containing information about a single song's worth of playback.
    /// </summary>
    /// <param name="index">The index of this song (in a created list).</param>
    /// <param name="json">The JSON object for this song (from Spotify data dump).</param>
    /// <exception cref="Exception"></exception>
    public SpotifyEntry(int index, JObject json)
    {
        try
        {
            Index = index;
            Json = json;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing contents of JSON tag:\n{json}\nto a valid song entry:\n{ex}");
        }
    }

    /// <summary>
    /// Unique identifier of this SpotifyEntry in a list
    /// </summary>
    public readonly int Index { get; }

    /// <summary>
    /// The JSON object containing the Spotify data from which this entry was derived.
    /// </summary>
    public readonly JObject Json { get; }

    /// <summary>
    /// The time the song started (<see cref="TimeStartedEst"/>) or ended (<see cref="TimeEnded"/>).
    /// </summary>
    public readonly DateTimeOffset Time => UseEstStartTime ? TimeStartedEst : TimeEnded;

    /// <summary>
    /// Determines whether or not to use <see cref="TimeStartedEst"/> as the reference time.
    /// </summary>
    public static bool UseEstStartTime => PreferEstimatedStartTime;

    /// <summary>
    /// This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time).
    /// </summary>
    public readonly DateTimeOffset TimeEnded => (DateTimeOffset?)Json["endTime"] ?? (DateTimeOffset?)Json["ts"] ?? throw new Exception("");

    /// <summary>
    /// The estimated time and date when the song started.
    /// Can be used in place of <see cref="TimeEnded"/> (provided by Spotify) if you prefer the pairings be based on when the song began.
    /// </summary>
    public readonly DateTimeOffset TimeStartedEst => TimeEnded - TimePlayed;

    /// <summary>
    /// This field is your Spotify username.
    /// </summary>
    public readonly string? Spotify_Username => (string?)Json["username"];

    /// <summary>
    /// This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
    /// </summary>
    public readonly string? Spotify_Platform => (string?)Json["platform"];

    /// <summary>
    /// This field is the number of milliseconds the stream was played.
    /// </summary>
    private readonly double Time_Played => (double?)Json["msPlayed"] ?? (double?)Json["ms_played"] ?? throw new Exception("");

    /// <summary>
    /// The duration of playback of this song, parsed from <see cref="Time_Played"/>.
    /// </summary>
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(Time_Played);

    /// <summary>
    /// This field is the country code of the country where the stream was played (e.g. SE - Sweden).
    /// </summary>
    public readonly string? Spotify_Country => (string?)Json["conn_country"];

    /// <summary>
    /// This field contains the IP address logged when streaming the track.
    /// </summary>
    public readonly string? Spotify_IP => (string?)Json["ip_addr_decrypted"];

    /// <summary>
    /// This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari).
    /// </summary>
    public readonly string? Spotify_UA => (string?)Json["user_agent_decrypted"];

    /// <summary>
    /// This field is the name of the track.
    /// </summary>
    public readonly string? Song_Name => (string?)Json["trackName"] ?? (string?)Json["master_metadata_track_name"];

    /// <summary>
    /// This field is the name of the artist, band or podcast.
    /// </summary>
    public readonly string? Song_Artist => (string?)Json["artistName"] ?? (string?)Json["master_metadata_album_artist_name"];

    /// <summary>
    /// This field is the name of the album of the track.
    /// </summary>
    public readonly string? Song_Album => (string?)Json["master_metadata_album_album_name"];

    /// <summary>
    /// A Spotify URI, uniquely identifying the track in the form of spotify:track:base-62 string.
    /// </summary>
    public readonly string? Song_URI => (string?)Json["spotify_track_uri"];

    /// <summary>
    /// The base-62 identifier found at the end of the Spotify URI, parsed from <see cref="Song_URI"/>.
    /// </summary>
    public readonly string? Song_ID => Song_URI?.Split(':', 3)[2];

    /// <summary>
    /// This field contains the name of the episode of the podcast.
    /// </summary>
    public readonly string? Episode_Name => (string?)Json["episode_name"];

    /// <summary>
    /// This field contains the name of the show of the podcast.
    /// </summary>
    public readonly string? Episode_Show => (string?)Json["episode_show_name"];

    /// <summary>
    /// A Spotify Episode URI, uniquely identifying the podcast episode in the form of spotify:episode:base-62 string.
    /// </summary>
    public readonly string? Episode_URI => (string?)Json["spotify_episode_uri"];

    /// <summary>
    /// This field is a value telling why the track started (e.g. “trackdone”).
    /// </summary>
    public readonly string? Song_StartReason => (string?)Json["reason_start"];

    /// <summary>
    /// This field is a value telling why the track ended (e.g. “endplay”).
    /// </summary>
    public readonly string? Song_EndReason => (string?)Json["reason_end"];

    /// <summary>
    /// This field has the value True or False depending on if shuffle mode was used when playing the track.
    /// </summary>
    public readonly bool? Song_Shuffle => (bool?)Json["shuffle"];

    /// <summary>
    /// This field indicates if the user skipped to the next song.
    /// </summary>
    public readonly bool? Song_Skipped => (bool?)Json["skipped"];

    /// <summary>
    /// This field indicates whether the track was played in offline mode (“True”) or not (“False”).
    /// </summary>
    public readonly bool? Spotify_Offline => (bool?)Json["offline"];

    /// <summary>
    /// This field is a timestamp of when offline mode was used, if used.
    /// </summary>
    private readonly string? Spotify_OfflineTS => (string?)Json["offline_timestamp"];

    /// <summary>
    /// The time and date of when offline mode was used, parsed from <see cref="Spotify_OfflineTS"/>.
    /// </summary>
    public readonly DateTimeOffset? OfflineTimestamp
    {
        get
        {
            if (Spotify_OfflineTS != null)
            {
                // Parse unix epoch to readable time
                return DateTimeOffset.FromUnixTimeSeconds(long.Parse(Spotify_OfflineTS));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// This field indicates whether the track was played in incognito mode (“True”) or not (“False”).
    /// </summary>
    public readonly bool? Spotify_Incognito => (bool?)Json["incognito"];

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

    public override int GetHashCode() => HashCode.Combine(Index, Name, Type);

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
    public GPXPoint(int index, Coordinate point, DateTimeOffset time)
    {
        Index = index;
        Location = point;
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
    public Coordinate(double lat, double lon)
    {
        Latitude = lat;
        Longitude = lon;
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

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public static bool operator ==(Coordinate c1, Coordinate c2) => c1.Equals(c2);

    public static bool operator !=(Coordinate c1, Coordinate c2) => !c1.Equals(c2);

    public static Coordinate operator +(Coordinate c1, Coordinate c2)
    {
        double latSum = c1.Latitude + c2.Latitude;
        double lonSum = c1.Longitude + c2.Longitude;
        return new Coordinate(latSum, lonSum);
    }

    public static Coordinate operator -(Coordinate c1, Coordinate c2)
    {
        double latDiff = c1.Latitude - c2.Latitude;
        double lonDiff = c1.Longitude - c2.Longitude;
        return new Coordinate(latDiff, lonDiff);
    }

    public static Coordinate operator *(Coordinate c, double scalar)
    {
        double latScaled = c.Latitude * scalar;
        double lonScaled = c.Longitude * scalar;
        return new Coordinate(latScaled, lonScaled);
    }

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

            string activity = SpotifyEntry.UseEstStartTime ? "started (est)" : "ended";

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