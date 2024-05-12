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
    /// Gets all pairs in the file.
    /// </summary>
    /// <returns>A list of Song-Point (SongPoint) pair objects.</returns>
    public List<SongPoint> GetAllPairs();

    /// <summary>
    /// The number of pairs in the source file.
    /// </summary>
    int SourcePairCount { get; }

    /// <summary>
    /// The number of pairs parsed successfuly from the source file.
    /// </summary>
    int ParsedPairCount { get; }
}
