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
}
