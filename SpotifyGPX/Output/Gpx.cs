// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the GPX format.
/// </summary>
public class Gpx : IFileOutput
{
    private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0"; // Namespace of the output GPX
    private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance"; // XML schema location of the output GPX
    private static string Schema => "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd"; // GPX schema location(s) of the output GPX
    private static string Waypoint => "wpt"; // Name of a waypoint object
    private XDocument Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the GPX format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    public Gpx(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    /// <summary>
    /// Creates an XDocument containing each pair, in GPX format.
    /// </summary>
    /// <param name="pairs">A list of pairs.</param>
    /// <returns>An XDocument containing the contents of the created GPX.</returns>
    private static XDocument GetDocument(IEnumerable<SongPoint> pairs)
    {
        var gpxPairs = pairs.Select(pair =>
            new XElement(Namespace + Waypoint,
                new XAttribute("lat", pair.Point.Location.Latitude),
                new XAttribute("lon", pair.Point.Location.Longitude),
                new XElement(Namespace + "name", pair.Song.ToString()),
                new XElement(Namespace + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
                new XElement(Namespace + "desc", pair.Description)
            )
        );

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Namespace + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Xsi),
                new XAttribute("xmlns", Namespace),
                new XAttribute(Xsi + "schemaLocation", Schema),
                new XElement(Namespace + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.GpxOutput)),
                gpxPairs
            )
        );
    }

    /// <summary>
    /// Saves this GPX file to the provided path.
    /// </summary>
    /// <param name="path">The path where this GPX file will be saved.</param>
    public void Save(string path)
    {
        Document.Save(path);
    }

    /// <summary>
    /// The number of waypoints (pairs) within this GPX file.
    /// </summary>
    public int Count => Document.Descendants(Namespace + Waypoint).Count(); // Number of point elements
}
