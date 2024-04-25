// SpotifyGPX by Simon Field

namespace SpotifyGPX.Api;

/// <summary>
/// Defines Spotify records capable of being used with the Spotify API.
/// </summary>
public interface ISpotifyApiCompat
{
    public string? Song_Name { get; }

    public string? Song_ID { get; }

    /// <summary>
    /// Non-null interpretation of this Spotify song's ID
    /// </summary>
    public string SongID => Song_ID ?? string.Empty;

    /// <summary>
    /// This song's Spotify API metadata layer for this track.
    /// </summary>
    public SpotifyApiEntry? Metadata { get; set; }
}
