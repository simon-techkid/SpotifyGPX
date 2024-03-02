// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the HTML format.
/// </summary>
public partial class Html : IFileOutput
{
    private XDocument Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the HTML format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    /// <param name="trackName">The name of the track representing the pairs.</param>
    public Html(IEnumerable<SongPoint> pairs, string trackName) => Document = GetDocument(pairs, trackName);

    /// <summary>
    /// Creates an XDocument containing each pair, in HTML format.
    /// </summary>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="trackName">The name of the track representing the pairs.</param>
    /// <returns>An XDocument containg the contents of the created HTML.</returns>
    private static XDocument GetDocument(IEnumerable<SongPoint> pairs, string trackName)
    {
        return new XDocument(
            new XDeclaration("1.0", DocumentEncoding, null),
            new XDocumentType("html", "-//W3C//DTD XHTML 1.1//EN", null, "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"),
            new XElement(Namespace + "html",
                new XAttribute("xmlns", Namespace),
                new XElement(Namespace + "head",
                    new XElement(Namespace + "meta",
                        new XAttribute("http-equiv", "Content-Type"),
                        new XAttribute("content", "text/html; charset=utf-8")),
                    new XElement(Namespace + "meta",
                        new XAttribute("name", "Generator"),
                        new XAttribute("content", "SpotifyGPX")),
                    new XElement(Namespace + "meta",
                        new XAttribute("name", "Author"),
                        new XAttribute("content", "SpotifyGPX")),
                    new XElement(Namespace + "title", $"{trackName} - SpotifyGPX"),
                    new XElement(Namespace + "style", new XAttribute("type", "text/css"), CSS)
                    ),
                new XElement(Namespace + "body",
                    new XElement(Namespace + "h1", trackName),
                    new XElement(Namespace + "hr"),
                    new XElement(Namespace + "ol", pairs.Select(pair => new XElement(Namespace + "li", pair.Song.ToString()))),
                    new XElement(Namespace + "hr")
                    )
                )
            );
    }

    /// <summary>
    /// Saves this HTML file to the provided path.
    /// </summary>
    /// <param name="path">The path where this GPX file will be saved.</param>
    public void Save(string path)
    {
        string doc = Document.ToString(OutputSettings);
        File.WriteAllText(path, doc, OutputEncoding);
    }

    /// <summary>
    /// The number of pairs within this HTML file.
    /// </summary>
    public int Count => Document.Descendants(Namespace + "li").Count(); // Number of point elements

}
