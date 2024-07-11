// SpotifyGPX by Simon Field

namespace SpotifyGPX.Api.Geocoding;

public interface IGpsMetadata
{
    /// <summary>
    /// Gets the string representation of the metadata.
    /// </summary>
    /// <returns>A string representation of the metadata.</returns>
    public string Stringify();
}
