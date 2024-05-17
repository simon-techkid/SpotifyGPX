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

    public abstract IPairInput.ParsePairsDelegate ParsePairsMethod { get; }
    public abstract IPairInput.FilterPairsDelegate FilterPairsMethod { get; }
    protected List<SongPoint> AllPairs => ParsePairsMethod();
    public abstract int SourcePairCount { get; }
    public virtual int ParsedPairCount => AllPairs.Count;

    // Songs

    public virtual ISongInput.ParseSongsDelegate ParseSongsMethod => GetAllSongs;
    public abstract ISongInput.FilterSongsDelegate FilterSongsMethod { get; }
    protected List<ISongEntry> AllSongs => AllPairs.Select(pair => pair.Song).ToList();
    protected List<ISongEntry> GetAllSongs() => AllSongs;
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllPairs.Select(pair => pair.Song).Count();

    // GPS Points

    public virtual IGpsInput.ParseTracksDelegate ParseTracksMethod => GetAllTracks;
    public abstract IGpsInput.FilterTracksDelegate FilterTracksMethod { get; }
    protected List<GpsTrack> AllTracks => AllPairs.GroupBy(pair => pair.Origin).Select(type => new GpsTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
    protected virtual List<GpsTrack> GetAllTracks() => AllTracks;
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => AllPairs.Select(pair => pair.Point).Count();
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => AllPairs.GroupBy(pair => pair.Origin).Count();
}
