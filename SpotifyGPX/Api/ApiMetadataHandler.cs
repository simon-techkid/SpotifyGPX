// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Api;

/// <summary>
/// A class for matching entries using their identifier (of type <typeparamref name="TIdentifier"/>) with their API metadata (of type <typeparamref name="TMetadata"/>) from the respective API service.
/// </summary>
/// <typeparam name="TImplementer">The type of object implementing <see cref="IApiMetadataRecordable{TIdentifier, TMetadata}"/>.</typeparam>
/// <typeparam name="TIdentifier">The type of the identifier of a <typeparamref name="TImplementer"/> object.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata object of a <typeparamref name="TImplementer"/> object.</typeparam>
public abstract class ApiMetadataHandler<TImplementer, TIdentifier, TMetadata, TCast> :
    StringBroadcasterBase
    where TIdentifier : notnull, IEquatable<TIdentifier>
    where TImplementer : IApiMetadataRecordable<TIdentifier, TMetadata>, TCast
{
    private readonly List<TImplementer> _entries;

    protected ApiMetadataHandler(List<TCast> entries, StringBroadcaster bcaster) : base(bcaster)
    {
        _entries = entries.OfType<TImplementer>().ToList();
    }

    /// <summary>
    /// Get a <see cref="Dictionary{TKey, TValue}"/> containing the metadata (of type <typeparamref name="TMetadata"/>) for the given identifiers (of type <typeparamref name="TIdentifier"/>).
    /// </summary>
    /// <param name="identifiers">An array of <typeparamref name="TIdentifier"/> objects to get metadata of type <typeparamref name="TMetadata"/> for.</param>
    /// <returns>A dictionary, where identifiers of type <typeparamref name="TIdentifier"/> are keys and metadata objects of type <typeparamref name="TMetadata"/> are values.</returns>
    protected abstract Dictionary<TIdentifier, TMetadata> GetMetadatas(List<TIdentifier> identifiers);

    /// <summary>
    /// Custom filter of returned identifiers of type <typeparamref name="TIdentifier"/>.
    /// </summary>
    protected virtual Func<TIdentifier, bool> CustomFilter => entry => true;

    /// <summary>
    /// Custom modifier of returned identifiers of type <typeparamref name="TIdentifier"/>, before they are processed.
    /// </summary>
    protected virtual Func<TIdentifier?, TIdentifier?> CustomModifier => entry => entry;

    /// <summary>
    /// The name of the entity being matched.
    /// </summary>
    protected virtual string NameOfEntity => "entity";

    /// <summary>
    /// The equality comparer for the identifiers of type <typeparamref name="TIdentifier"/>.
    /// Allows the identifiers to be compared for equality and filtered for uniqueness.
    /// </summary>
    protected virtual IEqualityComparer<TIdentifier> EqualityComparer => EqualityComparer<TIdentifier>.Default;

    /// <summary>
    /// Match the objects of type <typeparamref name="TImplementer"/> with their respective API metadata of type <typeparamref name="TMetadata"/>.
    /// </summary>
    /// <returns></returns>
    public List<TCast> MatchEntries()
    {
        List<TImplementer> entries = _entries;

        // Extract track IDs
        List<TIdentifier> trackIds = entries
            .Select(validEntry => CustomModifier(validEntry.GetEntryCode()))
            .Where(identifier => identifier != null && CustomFilter(identifier))
            .ToList()!;

        List<TIdentifier> distinctIds = trackIds.Distinct(EqualityComparer).ToList();
        BCaster.Broadcast($"Filtered {distinctIds.Count} unique {NameOfEntity} IDs from {trackIds.Count} total {NameOfEntity} IDs");

        // Get metadata from Spotify API
        Dictionary<TIdentifier, TMetadata> metadatas = GetMetadatas(distinctIds)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        BCaster.Broadcast($"Retrieved {metadatas.Count}/{distinctIds.Count} {NameOfEntity} metadata entries from API.");

        for (int i = 0; i < entries.Count; i++)
        {
            TImplementer entry = entries[i];

            if (entry.TryGetEntryCode(out TIdentifier entryCode) == true)
            {
                if (metadatas.TryGetValue(entryCode, out TMetadata? metadata))
                {
                    entry.Metadata = metadata;
                }
                else
                {
                    BCaster.BroadcastError(new Exception($"[API] No metadata found for {entry.ToString()} ({entryCode})"));
                }
            }

            entries[i] = entry;
        }

        return entries.Cast<TCast>().ToList();
    }

    /// <summary>
    /// Split an <see cref="IEnumerable{T}"/> into chunks of a given size.
    /// </summary>
    /// <typeparam name="T">The type of the chunks containing elements of type <typeparamref name="T"/>.</typeparam>
    /// <param name="source">The source <see cref="IEnumerable{T}"/> to split.</param>
    /// <param name="chunkSize">The size of each resulting chunk.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of chunks of size <paramref name="chunkSize"/>.</returns>
    protected static IEnumerable<IEnumerable<T>> SplitIntoChunks<T>(IEnumerable<T> source, int chunkSize)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }
}
