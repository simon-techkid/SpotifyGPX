// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

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

            builder.AppendLine("At this position: {0}", PointTime.ToString(Options.ISO8601Offset));
            builder.AppendLine("Song {0}", $"{Song.TimeName}: {SongTime.ToString(Options.ISO8601Offset)}");
            builder.AppendLine("Song Details:" + Environment.NewLine + "{0}", Song.Description);
            builder.AppendLine("Predicted Index: {0}", PredictedIndex != null ? PredictedIndex : null);

            return builder.ToString();
        }
    }

    /// <summary>
    /// Create a new <see cref="SongPoint"/> pairing.
    /// </summary>
    /// <param name="index">The index of this SongPoint (in a created list).</param>
    /// <param name="song">The ISongEntry (containing song data) of this pair's song.</param>
    /// <param name="point">The IGpsPoint (containing geospatial data) of this pair's point.</param>
    /// <param name="origin">The TrackInfo (track information) about the track from which this pair was created.</param>
    public SongPoint(int index, ISongEntry song, IGpsPoint point, TrackInfo origin)
    {
        Index = index;
        Song = song;
        Point = point;
        Origin = origin;
        PredictedIndex = null;
    }

    /// <summary>
    /// Creates a <see cref="SongPoint"/> pairing with a new Coordinate (lat/lon), based on an existing SongPoint pairing.
    /// </summary>
    /// <param name="oldPair">An existing <see cref="SongPoint"/> pairing.</param>
    /// <param name="newCoord">The new <see cref="Coordinate"/> for this <see cref="SongPoint"/>.</param>
    /// <param name="relIndex">The index of this prediction in a set of predictions.</param>
    public SongPoint(SongPoint oldPair, Coordinate newCoord, int relIndex) // Used for prediction only
    {
        this = oldPair;
        Point.Location = newCoord; // Create a GPXPoint using an existing point, with a new coordinate
        PredictedIndex = relIndex;
    }

    /// <summary>
    /// Unique identifier of this SongPoint in a list.
    /// </summary>
    public readonly int Index { get; }

    /// <summary>
    /// This song-point pair's song data.
    /// </summary>
    public readonly ISongEntry Song { get; }

    /// <summary>
    /// This song-point pair's point data.
    /// </summary>
    public readonly IGpsPoint Point { get; }

    /// <summary>
    /// Information about the track from which the point was created.
    /// </summary>
    public readonly TrackInfo Origin { get; }

    /// <summary>
    /// The total number of seconds between the song and the point.
    /// </summary>
    public readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds;

    /// <summary>
    /// Calculate the difference between two times.
    /// </summary>
    /// <param name="time1"></param>
    /// <param name="time2"></param>
    /// <returns>A <see cref="TimeSpan"/> representing a time interval between the provided times.</returns>
    public static TimeSpan DifferenceCalculator(DateTimeOffset time1, DateTimeOffset time2) => time1 - time2;

    /// <summary>
    /// Calculate the difference between two times.
    /// </summary>
    /// <param name="time1">The first time.</param>
    /// <param name="time2">The second time.</param>
    /// <returns>A <see langword="double"/> representing the difference in seconds between two times.</returns>
    public static double DisplacementCalculator(DateTimeOffset time1, DateTimeOffset time2) => DifferenceCalculator(time1, time2).TotalSeconds;

    /// <summary>
    /// Calculate the absolute difference between two times.
    /// </summary>
    /// <param name="time1">The first time.</param>
    /// <param name="time2">The second time.</param>
    /// <returns>A <see langword="double"/> representing the difference in seconds between two times.</returns>
    public static double DisplacementCalculatorAbs(DateTimeOffset time1, DateTimeOffset time2) => Math.Abs(DisplacementCalculator(time1, time2));

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
    public readonly DateTimeOffset SongTime => Song.Time.ToOffset(NormalizedOffset);

    /// <summary>
    /// The time and date the point was taken, converted to the pair's UTC offset (NormalizedOffset).
    /// </summary>
    public readonly DateTimeOffset PointTime => Point.Time.ToOffset(NormalizedOffset);

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
        // Set both the song and point times to the UTC offset provided by the original GPS point
        string songTime = SongTime.ToString(Options.TimeOnly);
        string pointTime = PointTime.ToString(Options.TimeOnly);

        // Print information about the pairing
        return $"[{Origin.ToString()}] [P{Point.Index}, S{Song.Index} ==> #{Index}] [{songTime}S ~ {pointTime}P] [{RoundAccuracy}s] {Song.ToString()}";
    }
}
