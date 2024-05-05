// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public sealed partial class Xspf : SongInputBase, IHashVerifier
{
    private XDocument Document { get; }
    protected override ParseSongsDelegate ParseSongsMethod => ParseSongs;
    protected override FilterSongsDelegate FilterSongsMethod => FilterSongs;

    public Xspf(string path)
    {
        Document = XDocument.Load(path, loadOptions);
    }

    private List<ISongEntry> ParseSongs()
    {
        return Document.Descendants(InputNs + Track).Select((element, index) => (ISongEntry)new XspfEntry
        {
            Index = index,
            CurrentInterpretation = Interpretation,
            FriendlyTime = DateTimeOffset.ParseExact(element.Element(InputNs + "annotation")?.Value ?? throw new Exception($"XSPF node {index} doesn't include a time value in the 'annotation' node"), Options.ISO8601UTC, null, TimeStyle),
            Time_Played = int.Parse(element.Element(InputNs + "duration")?.Value ?? throw new Exception($"XSPF node {index} doesn't include a duration value in the 'annotation' node")),
            Song_Name = element.Element(InputNs + "title")?.Value ?? throw new Exception($"XSPF node {index} doesn't include a song name in the 'title' node"),
            Song_Artist = element.Element(InputNs + "creator")?.Value ?? throw new Exception($"XSPF node {index} doesn't include a song artist in the 'creator' node"),
            Song_URI = element.Element(InputNs + "link")?.Value
        }).ToList();
    }

    private List<ISongEntry> FilterSongs()
    {
        return AllSongs.OfType<XspfEntry>().Where(song => filter(song)).Select(song => (ISongEntry)song).ToList();
    }

    public override int SourceSongCount => Document.Descendants(InputNs + Track).Count();

    public bool VerifyHash()
    {
        XmlHashProvider hasher = new();
        string? expectedHash = Document.Descendants(InputNs + "identifier").FirstOrDefault()?.Value;
        return hasher.VerifyHash(Document.Descendants(InputNs + Track), expectedHash);
    }
}
