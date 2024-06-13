// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public sealed partial class FolderedKml : XmlSaveable
{
    public override string FormatName => "kml";

    public FolderedKml(Func<IEnumerable<SongPoint>> pairs, string trackName) : base(pairs, trackName)
    {
    }

    protected override XDocument GetDocument(string? trackName)
    {
        IEnumerable<IGrouping<TrackInfo, SongPoint>> groups = GroupedDataProvider(pair => pair.Origin);

        IEnumerable<XElement> folders = groups
            .Select(group => new XElement(Namespace + Folder,
                new XElement(Namespace + "name", group.Key.ToString()),
                new XElement(Namespace + "description", group.Key.ToString()),
                group
                    .Select(pair =>
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
                    )
            ));

        string hash = HashProvider.ComputeHash(folders);

        return new XDocument(
            new XElement(Namespace + "kml",
                new XAttribute("xmlns", Namespace),
                new XAttribute(XNamespace.Xmlns + "gx", Gx),
                new XElement(Namespace + "Document",
                    new XElement(Namespace + "name", trackName),
                    new XElement(Namespace + "description", hash),
                    new XElement(Namespace + "snippet", DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)),
                    folders
                )
            )
        );
    }

    public override int Count => Document.Descendants(Namespace + Placemark).Count();
}
