// SpotifyGPX by Simon Field

using System.Collections.Generic;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with GPS input classes, unifying all formats accepting GPS journeys.
/// </summary>
public interface IGpsInput : IFileInput
{
    /// <summary>
    /// Gets all tracks in the file.
    /// </summary>
    /// <returns>A list of <see cref="GpsTrack"/> objects.</returns>
    List<GpsTrack> GetAllTracks();

    /// <summary>
    /// Gets tracks based on user-selection.
    /// </summary>
    /// <returns>A list of <see cref="GpsTrack"/> objects, based on user-selection</returns>
    List<GpsTrack> GetSelectedTracks();

    /// <summary>
    /// The total number of GPS tracks in the source file.
    /// </summary>
    int SourceTrackCount { get; }

    /// <summary>
    /// The total number of <see cref="IGpsPoint"/> points in the source file.
    /// </summary>
    int SourcePointCount { get; }

    /// <summary>
    /// The total number of GPS track objects parsed from the source file to <see cref="GpsTrack"/> objects.
    /// </summary>
    int ParsedTrackCount { get; }

    /// <summary>
    /// The total number of GPS points parsed from the source file to <see cref="IGpsPoint"/> objects.
    /// </summary>
    int ParsedPointCount { get; }
}
