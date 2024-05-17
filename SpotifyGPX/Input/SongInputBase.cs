// SpotifyGPX by Simon Field

using System.Collections.Generic;

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

    public abstract ISongInput.ParseSongsDelegate ParseSongsMethod { get; }

    public abstract ISongInput.FilterSongsDelegate FilterSongsMethod { get; }

    // All Songs
    protected List<ISongEntry> AllSongs => ParseSongsMethod();
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllSongs.Count;

    // Filtered Songs
    protected List<ISongEntry> FilteredSongs => FilterSongsMethod();
}
