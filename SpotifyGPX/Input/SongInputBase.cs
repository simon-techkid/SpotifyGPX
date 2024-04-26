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
    protected abstract List<ISongEntry> AllSongs { get; }
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllSongs.Count;
    public List<ISongEntry> GetAllSongs() => AllSongs;
    public List<ISongEntry> GetFilteredSongs() => FilterSongs();
    public List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTracks)
    {
        List<ISongEntry> FilteredSongs = FilterSongs(); // Filter songs based on file-specific filters first

        var trackRange = gpsTracks.Select(track => (track.Start, track.End)).ToList();

        // You may add other filtration options below, within the .Any() statement:

        FilteredSongs = AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End))) // Within the time range of tracks
            .ToList(); // Send the songs passing the filter to a list

        Console.WriteLine($"[INP] {FilteredSongs.Count} songs filtered from {AllSongs.Count} total");

        return FilteredSongs;
    }
    protected abstract List<ISongEntry> FilterSongs();
}
