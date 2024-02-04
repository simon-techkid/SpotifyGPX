using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public class Xspf
{
    private static XNamespace Namespace => "http://xspf.org/ns/0/"; // Namespace of output XSPF

    public Xspf(IEnumerable<SongPoint> pairs) => Document = GetXspfDocument(pairs);

    private XDocument Document { get; }

    private static XDocument GetXspfDocument(IEnumerable<SongPoint> Pairs)
    {
        return CreateXspf(Pairs.Select(song => ToXspf(song.Song)));
    }

    private static XDocument CreateXspf(IEnumerable<XElement> ele)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Namespace + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Namespace),
                new XElement(Namespace + "creator", "SpotifyGPX"),
                new XElement(Namespace + "trackList", ele)
            )
        );
    }

    private static XElement ToXspf(SpotifyEntry song)
    {
        return new XElement(Namespace + "track",
            new XElement(Namespace + "creator", song.Song_Artist),
            new XElement(Namespace + "title", song.Song_Name),
            new XElement(Namespace + "annotation", song.Time.UtcDateTime.ToString(Options.GpxOutput)),
            new XElement(Namespace + "duration", song.Time_Played) // use TimeSpan instead of this later, add Options format
        );
    }

    private int Count => Document.Descendants(Namespace + "track").Count();

    public void Save(string path)
    {
        Document.Save(path);
        Console.WriteLine(ToString());
    }

    public override string ToString() => $"[FILE] XSPF file containing {Count} points saved!";
}
