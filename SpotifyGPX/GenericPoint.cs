// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

public struct GenericPoint : IGpsPoint
{
    public int Index { get; set; }
    public Coordinate Location { get; set; }
    public DateTimeOffset Time { get; set; }
}
