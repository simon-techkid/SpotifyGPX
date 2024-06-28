// SpotifyGPX by Simon Field

namespace SpotifyGPX.SongInterfaces;

/// <summary>
/// Provides access to URLs of songs.
/// </summary>
public interface IUrlLinkableSong
{
    public string? Song_URL { get; }
    public string SongURL => Song_URL ?? string.Empty;
}
