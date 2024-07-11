// SpotifyGPX by Simon Field

namespace SpotifyGPX.Api;

/// <summary>
/// Defines an object that contains a metadata sidecar (metadata often retrieved from an API) of type <typeparamref name="TMetadata"/>.
/// </summary>
/// <typeparam name="TIdentifier">The type of the identifier (ID) for use when accessing metadata databases (often APIs).</typeparam>
/// <typeparam name="TMetadata">The type of the metadata record where the corresponding metadata will be stored.</typeparam>
public interface IApiMetadataRecordable<TIdentifier, TMetadata> : IEntryMatchable<TIdentifier>
{
    /// <summary>
    /// This entry's API metadata layer.
    /// </summary>
    public TMetadata? Metadata { get; set; }

    public string ToString();
}