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
    /// Gets all Song-Point pairings in the file, as <see cref="SongPoint"/> objects.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of Song-Point (<see cref="SongPoint"/>) pair objects.</returns>
    public List<SongPoint> GetAllPairs() => ParsePairsMethod();

    /// <summary>
    /// A <see langword="delegate"/> providing a method for parsing pairs as <see cref="SongPoint"/> objects from the source file.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="SongPoint"/> objects.</returns>
    public delegate List<SongPoint> ParsePairsDelegate();

    /// <summary>
    /// A <see langword="delegate"/> providing a method for parsing and filtering pairs as <see cref="SongPoint"/> objects from the source file.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="SongPoint"/> objects.</returns>
    public delegate List<SongPoint> FilterPairsDelegate();

    /// <summary>
    /// Provides access to the method that parses the pairs as <see cref="SongPoint"/> objects from the file.
    /// </summary>
    ParsePairsDelegate ParsePairsMethod { get; }

    /// <summary>
    /// Provides access to the method that parses and filters the pairs as <see cref="SongPoint"/> objects from the file.
    /// </summary>
    FilterPairsDelegate FilterPairsMethod { get; }

    /// <summary>
    /// The number of pairs in the source file.
    /// </summary>
    int SourcePairCount { get; }

    /// <summary>
    /// The number of pairs parsed successfully to <see cref="SongPoint"/> objects from the source file.
    /// </summary>
    int ParsedPairCount { get; }
}
