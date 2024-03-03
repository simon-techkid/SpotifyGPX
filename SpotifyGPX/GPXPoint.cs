// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;

namespace SpotifyGPX;

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
