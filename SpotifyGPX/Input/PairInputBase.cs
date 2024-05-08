// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of song-point pairing files. All classes that handle song-point pairing files must inherit this class.
/// </summary>
public abstract class PairInputBase : FileInputBase, ISongInput, IGpsInput, IPairInput
{
    protected PairInputBase(string path) : base(path)
    {
    }

    // Pairs
    protected delegate List<SongPoint> ParsePairsDelegate();
    protected abstract ParsePairsDelegate ParsePairsMethod { get; }
    protected List<SongPoint> AllPairs => ParsePairsMethod();
    public List<SongPoint> GetAllPairs() => AllPairs;
    public abstract int SourcePairCount { get; }
    public int ParsedPairCount => AllPairs.Count;

    // Songs
    public List<ISongEntry> GetAllSongs() => AllPairs.Select(pair => pair.Song).ToList();
    protected delegate List<ISongEntry> FilterSongsDelegate();
    protected abstract FilterSongsDelegate FilterSongsMethod { get; }
    public List<ISongEntry> GetFilteredSongs() => FilterSongsMethod();
    public List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTracks)
    {
        List<ISongEntry> FilteredSongs = GetFilteredSongs(); // Filter songs based on file-specific filters first

        var trackRange = gpsTracks.Select(track => (track.Start, track.End)).ToList();

        // You may add other filtration options below, within the .Any() statement:

        FilteredSongs = AllPairs.Where(pair => // If the spotify entry
            trackRange.Any(trackTimes => pair.Song.WithinTimeFrame(trackTimes.Start, trackTimes.End))).Select(pair => pair.Song) // Within the time range of tracks
            .ToList(); // Send the songs passing the filter to a list

        Console.WriteLine($"[INP] {FilteredSongs.Count} songs filtered from {ParsedSongCount} total");

        return FilteredSongs;
    }

    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllPairs.Select(pair => pair.Song).Count();

    // GPS
    public List<GpsTrack> GetAllTracks() => AllPairs.GroupBy(pair => pair.Origin).Select(type => new GpsTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => AllPairs.Select(pair => pair.Point).Count();
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => AllPairs.GroupBy(pair => pair.Origin).Count();
}
