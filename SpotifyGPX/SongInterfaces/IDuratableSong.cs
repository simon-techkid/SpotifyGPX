// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

/// <summary>
/// Provides access to song playback durations.
/// </summary>
public interface IDuratableSong
{
    public TimeSpan TimePlayed { get; }
}
