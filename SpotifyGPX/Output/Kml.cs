// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public sealed partial class Kml : XmlSaveable
{
    public override string FormatName => nameof(Kml).ToLower();
    protected override DocumentAccessor SaveAction => GetDocument;

    public Kml(Func<IEnumerable<SongPoint>> pairs, string trackName) : base(pairs, trackName)
    {
    }

    private XDocument GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> pairs = DataProvider();

        var kmlPairs = pairs.Select(pair =>
            new XElement(Namespace + Placemark,
                new XElement(Namespace + "name", pair.Song.ToString()),
                new XElement(Namespace + "description", pair.Description),
                new XElement(Namespace + "Point",
                    new XElement(Namespace + "coordinates", $"{pair.Point.Location.Longitude},{pair.Point.Location.Latitude}")
                ),
                new XElement(Namespace + "TimeStamp",
                    new XElement(Namespace + "when", pair.Point.Time.UtcDateTime.ToString(Options.ISO8601UTC))
                )
            )
        );

        XmlHashProvider hasher = new();
        string hash = hasher.ComputeHash(kmlPairs);

        return new XDocument(
            new XElement(Namespace + "kml",
                new XAttribute("xmlns", Namespace),
                new XAttribute(XNamespace.Xmlns + "gx", Gx),
                new XElement(Namespace + "Document",
                    new XElement(Namespace + "name", trackName),
                    new XElement(Namespace + "description", hash),
                    new XElement(Namespace + "snippet", DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)),
                    kmlPairs
                )
            )
        );
    }

    public override int Count => Document.Descendants(Namespace + Placemark).Count();
}
