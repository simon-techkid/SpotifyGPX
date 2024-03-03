// SpotifyGPX by Simon Field

namespace SpotifyGPX.Input;

/// <summary>
/// A list of the accepted formats containing song records.
/// </summary>
public enum SongFormats
{
    /// <summary>
    /// A JSON file containing user playback data in the Spotify format.
    /// </summary>
    Json,

    /// <summary>
    /// A .jsonreport file created by SpotifyGPX that can be used as input.
    /// </summary>
    JsonReport
}
