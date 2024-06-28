// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.PointEntry;

public struct KmlPoint : IGpsPoint
{
    public KmlPoint(int index, Coordinate location, DateTimeOffset time)
    {
        Index = index;
        Location = location;
        Time = time;
    }

    public int Index { get; }
    public Coordinate Location { get; set; }
    public DateTimeOffset Time { get; set; }
}
