// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public partial class Xspf : ISongInput
{
    private XDocument Document { get; }
    private List<SpotifyEntry> AllSongs { get; }

    public Xspf(string path)
    {
        Document = XDocument.Load(path);

        if (SongCount == 0)
        {
            // If there are no tracks in the GPX, throw error
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        AllSongs = ParseSongs();
    }

    public List<SpotifyEntry> ParseSongs()
    {
        return Document.Descendants(InputNs + Track).Select((element, index) => new SpotifyEntry(
            index,
            DateTimeOffset.ParseExact(element.Element(InputNs + "annotation")?.Value, Options.ISO8601UTC, null, TimeStyle),
            null,
            null,
            double.Parse(element.Element(InputNs + "duration")?.Value),
            null,
            null,
            null,
            element.Element(InputNs + "title")?.Value,
            element.Element(InputNs + "creator")?.Value,
            null,
            element.Element(InputNs + "link")?.Value,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
            ).ToList();
    }

    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    /// <summary>
    /// The total number of songs in the XSPF file.
    /// </summary>
    public int SongCount => Document.Descendants(InputNs + Track).Count();
}
