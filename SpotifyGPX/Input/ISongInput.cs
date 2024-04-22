// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with song input classes, unifying all formats accepting song records.
/// </summary>
public partial interface ISongInput
{
    /// <summary>
    /// Gets all songs in the file.
    /// </summary>
    /// <returns>A list of SpotifyEntry objects.</returns>
    List<ISongEntry> GetAllSongs();

    /// <summary>
    /// Get songs based on an existing list of track times.
    /// </summary>
    /// <param name="tracks">A list of GPXTrack objects.</param>
    /// <returns>A list of SpotifyEntry objects that must be within the times of the GPXTrack object(s).</returns>
    List<ISongEntry> GetFilteredSongs(List<GpsTrack> tracks)
    {
        List<ISongEntry> AllSongs = GetAllSongs();

        var trackRange = tracks.Select(track => (track.Start, track.End)).ToList();

        // FilterEntries() differs from AllSongs because it filters the entire JSON file by the following parameters:
        // The song must have been played during the GPS tracking timeframe (but PairingsHandler.PairPoints() filters this too)
        // The song must have been played for longer than the MinimumPlaytime TimeSpan (beginning of this file)
        // The song must have not been skipped during playback by the user (if ExcludeSkipped is true)

        // You may add other filtration options below, within the .Any() statement:

        List<ISongEntry> FilteredSongs = AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End))) // Within the time range of tracks
            .ToList(); // Send the songs passing the filter to a list

        Console.WriteLine($"[INP] {FilteredSongs.Count} songs filtered from {AllSongs.Count} total");

        return FilteredSongs;
    }

    /// <summary>
    /// The total number of songs in the given file.
    /// </summary>
    int SourceSongCount { get; }

    /// <summary>
    /// The total number of SpotifyEntry objects parsed from the given file.
    /// </summary>
    int ParsedSongCount { get; }
}
