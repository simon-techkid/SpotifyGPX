// SpotifyGPX by Simon Field

using System.Collections.Generic;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with song input classes, unifying all formats accepting song records.
/// </summary>
public partial interface ISongInput
{
    /// <summary>
    /// Gets all songs in the file.
    /// </summary>
    /// <returns>A list of <see cref="ISongEntry"/> objects, each representing the playback record of a song.</returns>
    List<ISongEntry> GetAllSongs();

    /// <summary>
    /// Filters the songs in the file by the song format's specified filter parameters.
    /// </summary>
    /// <returns>A list of <see cref="ISongEntry"/> objects, each representing the playback record of a song.</returns>
    List<ISongEntry> GetFilteredSongs();

    /// <summary>
    /// Filters the songs in the file by ensuring the returned songs' <see cref="ISongEntry.Time"/> fall within the provided <see cref="GpsTrack"/> objects.
    /// </summary>
    /// <param name="gpsTrack">The tracks whose <see cref="GpsTrack.Start"/> and <see cref="GpsTrack.End"/> will filter the <see cref="ISongEntry.Time"/>.</param>
    /// <returns>A list of <see cref="ISongEntry"/> objects, each representing the playback record of a song.</returns>
    List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTrack);

    /// <summary>
    /// The total number of songs in the given file.
    /// </summary>
    int SourceSongCount { get; }

    /// <summary>
    /// The total number of SpotifyEntry objects parsed from the given file.
    /// </summary>
    int ParsedSongCount { get; }
}
