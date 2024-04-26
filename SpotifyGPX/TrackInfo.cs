// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

/// <summary>
/// Metadata for a <see cref="GpsTrack"/> object.
/// </summary>
public readonly struct TrackInfo : IEquatable<TrackInfo>
{
    /// <summary>
    /// Creates a TrackInfo object for holding <see cref="GpsTrack"/> information.
    /// </summary>
    /// <param name="index">The index of this track (in a series of tracks).</param>
    /// <param name="name">The friendly name of this track.</param>
    /// <param name="type">The type of this track (GPX, Gap, or Combined).</param>
    public TrackInfo(int? index, string? name, TrackType type)
    {
        Indexx = index;
        NodeName = name;
        Type = type;
    }

    /// <summary>
    /// The index of this track (as provided to the constructor).
    /// </summary>
    private readonly int? Indexx { get; }

    /// <summary>
    /// The index of this track in a series of tracks.
    /// If no index, a generic index (based on <see cref="Type"/>) will be used.
    /// </summary>
    public readonly int Index => Indexx == null ? (int)Type : (int)Indexx;

    /// <summary>
    /// The name of this track (as provided to the constructor).
    /// </summary>
    private readonly string? NodeName { get; }

    /// <summary>
    /// The friendly name of this track.
    /// If no name, a generic name will be used.
    /// </summary>
    public readonly string Name => NodeName ?? $"T{Index}";

    /// <summary>
    /// The type of track represented (GPX, Gap, or Combined).
    /// </summary>
    public TrackType Type { get; }

    /// <summary>
    /// Converts this TrackInfo object to a string.
    /// </summary>
    /// <returns>The name of the track.</returns>
    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is TrackInfo other)
        {
            return Equals(other);
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Index, Name, Type);

    public bool Equals(TrackInfo other) => Index == other.Index && Name == other.Name && Type == other.Type;

    public static bool operator ==(TrackInfo left, TrackInfo right) => left.Equals(right);

    public static bool operator !=(TrackInfo left, TrackInfo right) => !(left == right);
}
