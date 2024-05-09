// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of song-point pairing files. All classes that handle song-point pairing files must inherit this class.
/// </summary>
public abstract class PairInputBase : FileInputBase, ISongInput, IGpsInput, IPairInput
{
    protected PairInputBase(string path, Broadcaster bcast) : base(path, bcast)
    {
    }

    // Pairs

    public abstract List<SongPoint> ParsePairsMethod();
    public abstract List<SongPoint> FilterPairsMethod();
    protected List<SongPoint> AllPairs => ParsePairsMethod();
    public abstract int SourcePairCount { get; }
    public virtual int ParsedPairCount => AllPairs.Count;

    // Songs

    public List<ISongEntry> ParseSongsMethod() => AllSongs;
    public abstract List<ISongEntry> FilterSongsMethod();
    protected List<ISongEntry> AllSongs => AllPairs.Select(pair => pair.Song).ToList();
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllPairs.Select(pair => pair.Song).Count();

    // GPS Points

    public List<GpsTrack> ParseTracksMethod() => AllTracks;
    public abstract List<GpsTrack> FilterTracksMethod();
    protected List<GpsTrack> AllTracks => AllPairs.GroupBy(pair => pair.Origin).Select(type => new GpsTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => AllPairs.Select(pair => pair.Point).Count();
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => AllPairs.GroupBy(pair => pair.Origin).Count();
}
