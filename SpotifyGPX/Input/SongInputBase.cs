// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing song playback records files. All classes that handle song playback records files must inherit this class.
/// </summary>
public abstract partial class SongInputBase : FileInputBase, ISongInput
{
    protected SongInputBase(string path) : base(path)
    {
    }

    /// <summary>
    /// The default <see cref="TimeInterpretation"/>. Override this property to change the interpretation for this file.
    /// </summary>
    protected virtual TimeInterpretation Interpretation => DefaultInterpretation;

    /// <summary>
    /// A delegate providing access to all songs within this song input file class.
    /// </summary>
    /// <returns>A list of <see cref="ISongEntry"/> objects, each <see cref="ISongEntry"/> representing a song playback record.</returns>
    protected delegate List<ISongEntry> ParseSongsDelegate();

    /// <summary>
    /// A delegate providing access to songs within this song input file class that pass the file-specific filters.
    /// </summary>
    /// <returns>A list of <see cref="ISongEntry"/> objects, each <see cref="ISongEntry"/> representing a song playback record.</returns>
    protected delegate List<ISongEntry> FilterSongsDelegate();

    /// <summary>
    /// Provides access to all songs within this song input file.
    /// </summary>
    protected abstract ParseSongsDelegate ParseSongsMethod { get; }

    /// <summary>
    /// Provides access to songs within this song input file that pass the file-specific filters.
    /// </summary>
    protected abstract FilterSongsDelegate FilterSongsMethod { get; }

    // All Songs
    protected virtual List<ISongEntry> AllSongs => ParseSongsMethod();
    public virtual List<ISongEntry> GetAllSongs() => AllSongs;
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllSongs.Count;

    // Filtered Songs
    protected virtual List<ISongEntry> FilteredSongs => FilterSongsMethod();
    public virtual List<ISongEntry> GetFilteredSongs() => FilteredSongs;
    public virtual List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTracks)
    {
        List<ISongEntry> filtered = FilteredSongs; // Filter songs based on file-specific filters first

        var trackRange = gpsTracks.Select(track => (track.Start, track.End)).ToList();

        // You may add other filtration options below, within the .Any() statement:

        filtered = AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End))) // Within the time range of tracks
            .ToList(); // Send the songs passing the filter to a list

        Console.WriteLine($"[INP] {filtered.Count} songs filtered from {AllSongs.Count} total");

        return filtered;
    }
}
