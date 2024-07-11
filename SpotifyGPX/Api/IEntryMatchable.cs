// SpotifyGPX by Simon Field

namespace SpotifyGPX.Api;

/// <summary>
/// Provides access to the identifier of an entry.
/// </summary>
/// <typeparam name="TResult">The type of the entry identifier.</typeparam>
public interface IEntryMatchable<TResult>
{
    /// <summary>
    /// Gets the identifier of the entry.
    /// </summary>
    TResult? GetEntryCode();

    /// <summary>
    /// Tries to get the identifier of the entry.
    /// </summary>
    /// <param name="entryCode">The identifier of the entry.</param>
    /// <returns>True if the entry has an identifier, false otherwise.</returns>
    bool TryGetEntryCode(out TResult entryCode)
    {
        entryCode = GetEntryCode()!;
        return entryCode != null;
    }
}
