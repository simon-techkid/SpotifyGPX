using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public class Gpx : IFileOutput
{
    private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0"; // Namespace of the output GPX
    private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance"; // XML schema location of the output GPX
    private static string Schema => "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd"; // GPX schema location(s) of the output GPX
    private static string Waypoint => "wpt"; // Name of a waypoint object

    public Gpx(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private XDocument Document { get; }

    private XDocument GetDocument(IEnumerable<SongPoint> Pairs)
    {
        IEnumerable<XElement> gpxPairs = Pairs.Select(ToGPX); // Get list of <wpt> elements, one for each pair
        return CreateGpx(gpxPairs); // Create the final Xml document, with header and list of pairs as <wpt> elements
    }

    private XElement ToGPX(SongPoint pair)
    {
        return new XElement(Namespace + Waypoint,
            new XAttribute("lat", pair.Point.Location.Latitude),
            new XAttribute("lon", pair.Point.Location.Longitude),
            new XElement(Namespace + "name", pair.Song.ToString()),
            new XElement(Namespace + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
            new XElement(Namespace + "desc", pair.Description)
        );
    }

    private static XDocument CreateGpx(IEnumerable<XElement> gpxPairs)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Namespace + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Xsi),
                new XAttribute("xmlns", Namespace),
                new XAttribute(Xsi + "schemaLocation", Schema),
                new XElement(Namespace + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.GpxOutput)),
                gpxPairs // All pairs below header
            )
        );
    }

    public void Save(string path)
    {
        Document.Save(path);
    }

    public int Count => Document.Descendants(Namespace + Waypoint).Count();
}
