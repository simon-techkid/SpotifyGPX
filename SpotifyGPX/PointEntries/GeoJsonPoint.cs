// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

public struct GeoJsonPoint : IGpsPoint
{
    public int Index { get; set; }
    public string? Type { get; set; }
    public DateTimeOffset Time { get; set; }
    public string? PropertiesProvider { get; set; }
    public double? PropertiesAccuracy { get; set; }
    public double? PropertiesAltitude { get; set; }
    public double? PropertiesBearing { get; set; }
    public double? PropertiesSpeed { get; set; }
    public string? GeometryType { get; set; }
    public Coordinate GeometryCoordinates { get; private set; }
    public Coordinate Location
    {
        readonly get => GeometryCoordinates;
        set => GeometryCoordinates = value;
    }
}
