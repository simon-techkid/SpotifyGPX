// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public sealed partial class Xspf : XmlSaveable
{
    public override string FormatName => nameof(Xspf).ToLower();
    protected override DocumentAccessor SaveAction => GetDocument;

    public Xspf(Func<IEnumerable<SongPoint>> pairs, string trackName) : base(pairs, trackName)
    {
    }

    private XDocument GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> pairs = DataProvider();

        var xspfPairs = pairs.Select(pair =>
        new XElement(Namespace + Track,
            CheckNullNode(Namespace + "title", pair.Song.Song_Name),
            CheckNullNode(Namespace + "creator", pair.Song.Song_Artist),
            CheckNullNode(Namespace + "annotation", pair.Song.Time.UtcDateTime.ToString(Options.ISO8601UTC)),
            CheckNullNode(Namespace + "album", pair.Song.GetPropertyValue<IAlbumableSong>(song => song.SongAlbum)),
            CheckNullNode(Namespace + "duration", pair.Song.GetPropertyValue<IDuratableSong>(song => song.TimePlayed.TotalMilliseconds)),
            CheckNullNode(Namespace + "link", pair.Song.GetPropertyValue<IUrlLinkableSong>(song => song.SongURL))
        ));

        string hash = HashProvider.ComputeHash(xspfPairs);

        return new XDocument(
            new XElement(Namespace + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Namespace),
                new XElement(Namespace + "title", trackName),
                new XElement(Namespace + "creator", "SpotifyGPX"),
                new XElement(Namespace + "annotation", Comment),
                new XElement(Namespace + "identifier", hash),
                new XElement(Namespace + "date", DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)),
                new XElement(Namespace + "trackList", xspfPairs) // All pairs inside <trackList>
            )
        );
    }

    private static XElement? CheckNullNode(XName nodeName, object? content)
    {
        if (content != null)
            return new XElement(nodeName, content);

        return null;
    }

    public override int Count => Document.Descendants(Namespace + Track).Count(); // Number of track elements
}