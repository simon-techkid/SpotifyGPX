// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with song input classes, unifying all formats accepting song records.
/// </summary>
public partial interface ISongInput : IDisposable
{
    /// <summary>
    /// Provides access to the method that parses the <see cref="ISongEntry"/> objects from the file.
    /// </summary>
    public List<ISongEntry> ParseSongsMethod();

    /// <summary>
    /// Gets all songs as <see cref="ISongEntry"/> objects from the file.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="ISongEntry"/> objects, each representing the playback record of a song.</returns>
    public List<ISongEntry> GetAllSongs() => ParseSongsMethod();

    /// <summary>
    /// Provides access to the method that parses and filters the <see cref="ISongEntry"/> objects with file-specific filters.
    /// </summary>
    public List<ISongEntry> FilterSongsMethod();

    /// <summary>
    /// Gets filtered songs as <see cref="ISongEntry"/> objects in the file using the song format's specified filter parameters.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="ISongEntry"/> objects, each representing the playback record of a song.</returns>
    public List<ISongEntry> GetFilteredSongs() => FilterSongsMethod();

    /// <summary>
    /// Filters the songs in the file by ensuring the returned songs' <see cref="ISongEntry.Time"/> fall within the provided <see cref="GpsTrack"/> objects.
    /// </summary>
    /// <param name="gpsTracks">The tracks whose <see cref="GpsTrack.Start"/> and <see cref="GpsTrack.End"/> will filter the <see cref="ISongEntry.Time"/>.</param>
    /// <returns>A list of <see cref="ISongEntry"/> objects, each representing the playback record of a song.</returns>
    public List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTracks)
    {
        List<ISongEntry> filtered = GetFilteredSongs(); // Filter songs based on file-specific filters first

        var trackRange = gpsTracks.Select(track => (track.Start, track.End)).ToList();

        // You may add other filtration options below, within the .Any() statement:

        filtered = filtered.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End))) // Within the time range of tracks
            .ToList(); // Send the songs passing the filter to a list

        Console.WriteLine($"[INP] {filtered.Count} songs filtered from {ParsedSongCount} total");

        return filtered;
    }

    public bool Disposed { get; }

    /// <summary>
    /// The total number of songs in the given file.
    /// </summary>
    public int SourceSongCount { get; }

    /// <summary>
    /// The total number of SpotifyEntry objects parsed from the given file.
    /// </summary>
    public int ParsedSongCount { get; }
}
