// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// A list of the supported export formats.
/// </summary>
public enum Formats
{
    /// <summary>
    /// A CSV file containing a pair per line.
    /// </summary>
    Csv,

    /// <summary>
    /// A GPX file containing song-point pairs as a waypoints.
    /// </summary>
    Gpx,

    /// <summary>
    /// A HTML file containing a visualized webpage of pairs.
    /// </summary>
    Html,

    /// <summary>
    /// A JSON file containing only the original Spotify data records used for pairs.
    /// </summary>
    Json,

    /// <summary>
    /// A .jsonreport file containing all pairing and track data.
    /// </summary>
    JsonReport,

    /// <summary>
    /// A plain text file containing a string per pair.
    /// </summary>
    Txt,

    /// <summary>
    /// An XML playlist file compatible with audio playback software.
    /// </summary>
    Xspf
}
