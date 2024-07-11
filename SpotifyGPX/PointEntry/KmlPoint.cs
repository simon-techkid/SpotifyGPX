// SpotifyGPX by Simon Field

using SpotifyGPX.Api.Geocoding;
using System;

namespace SpotifyGPX.PointEntry;

public struct KmlPoint : IGpsPoint
{
    public readonly string Description
    {
        get
        {
            StringBuilder builder = new();

            builder.AppendLine("Time Zone: {0}", Time.Offset.ToString(Options.TimeSpan));
            builder.AppendLine("Metadata: " + Environment.NewLine + "{0}", MetadataDesc);

            return builder.ToString();
        }
    }

    private readonly string? MetadataDesc
    {
        get
        {
            if (Metadata == null)
                return null;

            StringBuilder builder = new();

            builder.AppendLine("{0}", Metadata.Stringify());

            return builder.ToString();
        }
    }

    public KmlPoint(int index, Coordinate location, DateTimeOffset time)
    {
        Index = index;
        Location = location;
        Time = time;
    }

    public int Index { get; }
    public Coordinate Location { get; set; }
    public DateTimeOffset Time { get; set; }
    public IGpsMetadata? Metadata { get; set; }
    public readonly Coordinate GetEntryCode() => Location;
    public override readonly string ToString() => Location.ToString();
}
