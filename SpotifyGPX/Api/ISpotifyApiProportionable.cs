// SpotifyGPX by Simon Field

using SpotifyGPX.SongInterfaces;

namespace SpotifyGPX.Api;

/// <summary>
/// Provides instructions for determining what percent proportion of a song the user actually listened to.
/// </summary>
public interface ISpotifyApiProportionable : ISpotifyApiCompat, IDuratableSong
{
    /// <summary>
    /// The percent of the full track the user listened to.
    /// This field requires both the duration of the song the user played, as well as the duration of the full song, to be present in the implementing class.
    /// </summary>
    public decimal? PercentListened { get; }
}
