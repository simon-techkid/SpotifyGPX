// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public partial class Xspf : SongInputBase, IHashVerifier
{
    private XDocument Document { get; }
    protected override List<SpotifyEntry> AllSongs { get; }

    public Xspf(string path)
    {
        Document = XDocument.Load(path);

        if (SourceSongCount == 0)
        {
            // If there are no tracks in the GPX, throw error
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        AllSongs = ParseSongs();
    }

    private List<SpotifyEntry> ParseSongs()
    {
        return Document.Descendants(InputNs + Track).Select((element, index) => new SpotifyEntry
        {
            Index = index,
            TimeInterpretation = TimeInterpretation.End,
            Time = DateTimeOffset.ParseExact(element.Element(InputNs + "annotation")?.Value ?? throw new Exception($"XSPF node {index} doesn't include a time value in the 'annotation' node"), Options.ISO8601UTC, null, TimeStyle),
            Time_Played = int.Parse(element.Element(InputNs + "duration")?.Value ?? throw new Exception($"XSPF node {index} doesn't include a duration value in the 'annotation' node")),
            Song_Name = element.Element(InputNs + "title")?.Value,
            Song_Artist = element.Element(InputNs + "creator")?.Value,
            Song_URI = element.Element(InputNs + "link")?.Value
        }).ToList();
    }

    public override int SourceSongCount => Document.Descendants(InputNs + Track).Count();

    public bool VerifyHash()
    {
        XmlHashProvider hasher = new();
        string? expectedHash = Document.Descendants(InputNs + "identifier").FirstOrDefault()?.Value;
        return hasher.VerifyHash(Document.Descendants(InputNs + Track), expectedHash);
    }
}
