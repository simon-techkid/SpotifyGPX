// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with pair input classes, unifying all formats accepting pairs.
/// </summary>
public interface IPairInput : IDisposable
{
    /// <summary>
    /// Provides access to the method that parses the pairs as <see cref="SongPoint"/> objects from the file.
    /// </summary>
    public List<SongPoint> ParsePairsMethod();

    /// <summary>
    /// Gets all Song-Point pairings in the file, as <see cref="SongPoint"/> objects.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of Song-Point (<see cref="SongPoint"/>) pair objects.</returns>
    public List<SongPoint> GetAllPairs() => ParsePairsMethod();

    /// <summary>
    /// Provides access to the method that parses and filters the pairs as <see cref="SongPoint"/> objects from the file.
    /// </summary>
    public List<SongPoint> FilterPairsMethod();

    public bool Disposed { get; }

    /// <summary>
    /// The number of pairs in the source file.
    /// </summary>
    public int SourcePairCount { get; }

    /// <summary>
    /// The number of pairs parsed successfully to <see cref="SongPoint"/> objects from the source file.
    /// </summary>
    public int ParsedPairCount { get; }
}
