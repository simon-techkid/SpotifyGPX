// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public sealed partial class Gpx : XmlSaveable
{
    public override string FormatName => nameof(Gpx).ToLower();

    public Gpx(Func<IEnumerable<SongPoint>> pairs, string trackName) : base(pairs, trackName)
    {
    }

    protected override XDocument GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> pairs = DataProvider();

        var gpxPairs = pairs.Select(pair =>
            new XElement(Namespace + Waypoint,
                new XAttribute("lat", pair.Point.Location.Latitude),
                new XAttribute("lon", pair.Point.Location.Longitude),
                new XElement(Namespace + "name", pair.Song.ToString()),
                new XElement(Namespace + "time", pair.Point.Time.UtcDateTime.ToString(Options.ISO8601UTC)),
                new XElement(Namespace + "desc", pair.Description)
            )
        );

        string hash = HashProvider.ComputeHash(gpxPairs);

        return new XDocument(
            new XElement(Namespace + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Xsi),
                new XAttribute("xmlns", Namespace),
                new XAttribute(Xsi + "schemaLocation", Schema),
                new XElement(Namespace + "name", trackName),
                new XElement(Namespace + "desc", hash),
                new XElement(Namespace + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)),
                gpxPairs
            )
        );
    }

    public override int Count => Document.Descendants(Namespace + Waypoint).Count();
}
