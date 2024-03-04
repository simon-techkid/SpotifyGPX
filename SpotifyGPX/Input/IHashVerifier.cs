// SpotifyGPX by Simon Field

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with file import classes supporting verifying the included hashes with the subsequent data.
/// </summary>
public interface IHashVerifier
{
    /// <summary>
    /// Verifies whether checksum included in the document matches the document's contents.
    /// </summary>
    /// <returns>True, if the checksum matches. Otherwise, false.</returns>
    bool VerifyHash();
}
