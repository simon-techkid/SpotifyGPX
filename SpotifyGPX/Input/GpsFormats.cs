// SpotifyGPX by Simon Field

namespace SpotifyGPX.Input;

/// <summary>
/// A list of the accepted formats containing GPS journeys.
/// </summary>
public enum GpsFormats
{
    /// <summary>
    /// A GPX file containing geospatial information for a journey.
    /// </summary>
    Gpx,

    /// <summary>
    /// A .jsonreport file created by SpotifyGPX that can be used as input.
    /// </summary>
    JsonReport
}
