// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.SongInterfaces;

/// <summary>
/// Provides access to song playback durations.
/// </summary>
public interface IDuratableSong
{
    public TimeSpan TimePlayed { get; }
}
