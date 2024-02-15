// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public class Xspf : IFileOutput
{
    private static XNamespace Namespace => "http://xspf.org/ns/0/"; // Namespace of output XSPF
    private static string Track => "track"; // Name of a track object

    public Xspf(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private XDocument Document { get; }

    private static XDocument GetDocument(IEnumerable<SongPoint> Pairs)
    {
        IEnumerable<XElement> xspfPairs = Pairs.Select(pair => ToXspf(pair.Song)); // Get list of <track> elements, one for each pair's song
        return CreateXspf(xspfPairs); // Create the final Xml document, with header and list of songs as <track> objects
    }

    private static XElement ToXspf(SpotifyEntry song)
    {
        return new XElement(Namespace + Track,
            new XElement(Namespace + "creator", song.Song_Artist),
            new XElement(Namespace + "title", song.Song_Name),
            new XElement(Namespace + "annotation", song.Time.UtcDateTime.ToString(Options.GpxOutput)),
            new XElement(Namespace + "duration", song.TimePlayed?.TotalMilliseconds) // number of milliseconds
        );
    }

    private static XDocument CreateXspf(IEnumerable<XElement> xspfPairs)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Namespace + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Namespace),
                new XElement(Namespace + "creator", "SpotifyGPX"),
                new XElement(Namespace + "trackList", xspfPairs) // All pairs inside <trackList>
            )
        );
    }

    public void Save(string path)
    {
        Document.Save(path);
    }

    public int Count => Document.Descendants(Namespace + Track).Count(); // Number of track elements
}
