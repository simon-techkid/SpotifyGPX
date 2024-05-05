// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing song playback records files. All classes that handle song playback records files must inherit this class.
/// </summary>
public abstract class SongInputBase : ISongInput
{
    protected delegate List<ISongEntry> ParseSongsDelegate();
    protected delegate List<ISongEntry> FilterSongsDelegate();
    protected abstract ParseSongsDelegate ParseSongsMethod { get; }
    protected abstract FilterSongsDelegate FilterSongsMethod { get; }

    // All Songs
    protected List<ISongEntry> AllSongs => ParseSongsMethod();
    public List<ISongEntry> GetAllSongs() => AllSongs;
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllSongs.Count;

    // Filtered Songs
    protected List<ISongEntry> FilteredSongs => FilterSongsMethod();
    public List<ISongEntry> GetFilteredSongs() => FilteredSongs;
    public List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTracks)
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
