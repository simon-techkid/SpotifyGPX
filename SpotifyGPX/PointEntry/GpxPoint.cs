// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.PointEntry;

public struct GpxPoint : IGpsPoint
{
    public int Index { get; set; }
    public Coordinate Location { get; set; }
    public DateTimeOffset Time { get; set; }
}
