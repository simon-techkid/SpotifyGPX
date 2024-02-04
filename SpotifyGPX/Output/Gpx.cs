using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public class Gpx
{
    private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0"; // Namespace of the output GPX
    private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance"; // XML schema location of the output GPX
    private static string Schema => "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd"; // GPX schema location(s) of the output GPX

    public Gpx(IEnumerable<SongPoint> pairs, PlacementStyles style)
    {
        Document = GetDocument(pairs);
        DocumentStyle = style;
    }

    private XDocument Document { get; }

    private PlacementStyles DocumentStyle { get; }

    public enum PlacementStyles
    {
        wpt,
        trkpt
    }

    private XDocument GetDocument(IEnumerable<SongPoint> Pairs)
    {
        IEnumerable<XElement> elements;

        if (DocumentStyle == PlacementStyles.wpt)
        {
            elements = Pairs.Select(ToGPX);
        }
        else
        {
            elements = Pairs
                .GroupBy(pair => pair.Origin)
                .Select(track =>
                    new XElement(Namespace + "trk",
                        new XElement(Namespace + "name", track.Key.ToString()),
                        new XElement(Namespace + "trkseg", track.Select(pair => ToGPX(pair)))
                    )
                );
        }

        return CreateGpx(elements);
    }

    private static XDocument CreateGpx(IEnumerable<XElement> elements)
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
                elements
            )
        );
    }

    private XElement ToGPX(SongPoint pair)
    {
        return new XElement(Namespace + DocumentStyle.ToString(),
            new XAttribute("lat", pair.Point.Location.Latitude),
            new XAttribute("lon", pair.Point.Location.Longitude),
            new XElement(Namespace + "name", pair.Song.ToString()),
            new XElement(Namespace + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
            new XElement(Namespace + "desc", pair.Description)
        );
    }

    private int Count => Document.Descendants(Namespace + DocumentStyle.ToString()).Count();

    public void Save(string path)
    {
        Document.Save(path);
        Console.WriteLine(ToString());
    }

    public override string ToString() => $"[FILE] GPX file containing {Count} points saved!";
}
