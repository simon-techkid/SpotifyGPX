// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of song-point pairing files. All classes that handle song-point pairing files must inherit this class.
/// </summary>
public abstract class PairInputBase : GpsInputSelection, ISongInput, IGpsInput, IPairInput
{
    protected PairInputBase(string path) : base(path)
    {
    }

    // Pairs
    /// <summary>
    /// A delegate for the method that parses the pairs from the file.
    /// </summary>
    /// <returns>A list of <see cref="SongPoint"/> objects, each <see cref="SongPoint"/> representing a paired <see cref="ISongEntry"/> and <see cref="IGpsPoint"/>.</returns>
    protected delegate List<SongPoint> ParsePairsDelegate();

    /// <summary>
    /// Provides the method that parses the <see cref="SongPoint"/> pairs from the file.
    /// </summary>
    protected abstract ParsePairsDelegate ParsePairsMethod { get; }

    /// <summary>
    /// Access all pairs in this song-point pairing data file.
    /// </summary>
    protected virtual List<SongPoint> AllPairs => ParsePairsMethod();
    public virtual List<SongPoint> GetAllPairs() => AllPairs;
    public abstract int SourcePairCount { get; }
    public virtual int ParsedPairCount => AllPairs.Count;

    // Songs
    /// <summary>
    /// A delegate for the method that filters the songs based on the file-specific filters.
    /// </summary>
    /// <returns></returns>
    protected delegate List<ISongEntry> FilterSongsDelegate();

    /// <summary>
    /// Provides the method that filters the songs based on the file-specific filters.
    /// </summary>
    protected abstract FilterSongsDelegate FilterSongsMethod { get; }

    public virtual List<ISongEntry> GetAllSongs() => AllPairs.Select(pair => pair.Song).ToList();
    public virtual List<ISongEntry> GetFilteredSongs() => FilterSongsMethod();
    public virtual List<ISongEntry> GetFilteredSongs(List<GpsTrack> gpsTracks)
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
    public override List<GpsTrack> GetAllTracks() => AllPairs.GroupBy(pair => pair.Origin).Select(type => new GpsTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => AllPairs.Select(pair => pair.Point).Count();
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => AllPairs.GroupBy(pair => pair.Origin).Count();
}
