// SpotifyGPX by Simon Field

namespace SpotifyGPX;

/// <summary>
/// The list of possible types a journey track can represent.
/// </summary>
public enum TrackType
{
    /// <summary>
    /// A track created from original GPS data.
    /// </summary>
    GPX,

    /// <summary>
    /// A track created from gaps between GPS tracks.
    /// </summary>
    Gap,

    /// <summary>
    /// A single track containing all journey data combined (including gaps).
    /// </summary>
    Combined
}
