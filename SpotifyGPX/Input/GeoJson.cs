// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Input;

public sealed partial class GeoJson : GpsInputBase
{
    private JsonDocument Document { get; }
    protected override string FormatName => nameof(GeoJson);
    public override List<GpsTrack> ParseTracksMethod() => ParseTracks();
    public override List<GpsTrack> FilterTracksMethod() => FilterTracks();

    public GeoJson(string path, StringBroadcaster bcast) : base(path, bcast)
    {
        using JsonNetDeserializer deserializer = new(path, bcast);
        Document = deserializer.GetDocument();
    }

    private List<GpsTrack> ParseTracks()
    {
        JsonElement root = Document.RootElement;

        var featureCollection = new
        {
            Type = JsonTools.TryGetProperty("type", root)?.GetString(),
            Features = new List<IGpsPoint>()
        };

        JsonElement features = JsonTools.ForceGetProperty("features", root);

        features
            .EnumerateArray()
            .Select((feature, index) =>
            {
                string? type = feature.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : null;

                JsonElement properties = JsonTools.ForceGetProperty("properties", feature);
                string time = JsonTools.ForceGetProperty("time", properties).GetString() ?? throw new Exception("Time missing from GeoJSON feature");
                string? provider = JsonTools.TryGetProperty("provider", properties)?.GetString();
                double? accuracy = JsonTools.TryGetProperty("accuracy", properties)?.GetDouble();
                double? altitude = JsonTools.TryGetProperty("altitude", properties)?.GetDouble();
                double? bearing = JsonTools.TryGetProperty("bearing", properties)?.GetDouble();
                double? speed = JsonTools.TryGetProperty("speed", properties)?.GetDouble();

                JsonElement geometry = JsonTools.ForceGetProperty("geometry", feature);
                string? geometryType = JsonTools.TryGetProperty("type", geometry)?.GetString();
                double latitude = JsonTools.ForceGetProperty("coordinates", geometry)[1].GetDouble();
                double longitude = JsonTools.ForceGetProperty("coordinates", geometry)[0].GetDouble();

                IGpsPoint geoFeature = (GeoJsonPoint)new()
                {
                    Index = index,
                    Type = type,
                    Time = DateTimeOffset.Parse(time),
                    PropertiesProvider = provider,
                    PropertiesAccuracy = accuracy,
                    PropertiesAltitude = altitude,
                    PropertiesBearing = bearing,
                    PropertiesSpeed = speed,
                    GeometryType = geometryType,
                    Location = new Coordinate(latitude, longitude)
                };

                return geoFeature;
            })
            .ToList()
            .AddRange(featureCollection.Features);

        return featureCollection.Features.Select((feature, index) => new GpsTrack(null, null, TrackType.Gps, featureCollection.Features)).ToList();
    }

    private List<GpsTrack> FilterTracks()
    {
        return AllTracks.Where(track => track.OfType<GeoJsonPoint>().All(point => filter(point))).ToList();
    }

    public override int SourcePointCount => 1;
    public override int SourceTrackCount => 1;

    protected override void DisposeDocument()
    {
        Document.Dispose();
    }

}
