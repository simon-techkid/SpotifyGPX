// SpotifyGPX by Simon Field

using Newtonsoft.Json;
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
