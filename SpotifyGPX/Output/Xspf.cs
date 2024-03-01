// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the XSPF format.
/// </summary>
public class Xspf : IFileOutput
{
    private static XNamespace Namespace => "http://xspf.org/ns/0/"; // Namespace of output XSPF
    private static string Track => "track"; // Name of a track object
    private XDocument Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the XSPF format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    /// <param name="trackName">The name of the track representing the pairs.</param>
    public Xspf(IEnumerable<SongPoint> pairs, string trackName) => Document = GetDocument(pairs, trackName);

    /// <summary>
    /// Creates an XDocument containing each song of each pair, in XSPF format.
    /// </summary>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="trackName">The name of the track representing the pairs.</param>
    /// <returns>An XDocument containing the contents of the created XSPF.</returns>
    private static XDocument GetDocument(IEnumerable<SongPoint> pairs, string trackName)
    {
        var xspfPairs = pairs.Select(pair =>
        new XElement(Namespace + Track,
            new XElement(Namespace + "creator", pair.Song.Song_Artist),
            new XElement(Namespace + "title", pair.Song.Song_Name),
            new XElement(Namespace + "annotation", pair.Song.Time.UtcDateTime.ToString(Options.GpxOutput)),
            new XElement(Namespace + "duration", pair.Song.TimePlayed?.TotalMilliseconds) // number of milliseconds
        ));

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Namespace + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Namespace),
                new XElement(Namespace + "title", trackName),
                new XElement(Namespace + "creator", "SpotifyGPX"),
                new XElement(Namespace + "trackList", xspfPairs) // All pairs inside <trackList>
            )
        );
    }

    /// <summary>
    /// Saves this XSPF file to the provided path.
    /// </summary>
    /// <param name="path">The path where this XSPF will be saved.</param>
    public void Save(string path)
    {
        Document.Save(path);
    }

    /// <summary>
    /// The number of tracks (songs) within this XSPF file.
    /// </summary>
    public int Count => Document.Descendants(Namespace + Track).Count(); // Number of track elements
}
