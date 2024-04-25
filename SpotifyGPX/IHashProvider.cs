// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace SpotifyGPX;

/// <summary>
/// Unifies all classes supporting returning a hash of an object (as a string).
/// </summary>
/// <typeparam name="T">The type of the object being hashed.</typeparam>
public interface IHashProvider<T>
{
    string ComputeHash(T data);
}

/// <summary>
/// Unifies all classes supporting verifying a hash of an object with another hash.
/// </summary>
/// <typeparam name="T">The type of the object being hashed.</typeparam>
public interface IHashChecker<T>
{
    bool VerifyHash(T data, string expectedHash);
}

/// <summary>
/// Central class supporting creating and verifying hashes given a specific format.
/// </summary>
/// <typeparam name="T">The type of the object being hashed.</typeparam>
public abstract class FormatHashProviderBase<T> : IHashProvider<T>, IHashChecker<T>
{
    /// <summary>
    /// Serialize an object to a string so that the string can serve as the hashed payload.
    /// </summary>
    /// <param name="data">An object to be hashed.</param>
    /// <returns>The object as a string.</returns>
    protected abstract string SerializeData(T data);

    /// <summary>
    /// Calculates the SHA256 checksum hash for the given data.
    /// </summary>
    /// <param name="data">An object to be hashed.</param>
    /// <returns>A string representing the SHA256 checksum of the data.</returns>
    public string ComputeHash(T data)
    {
        string serializedData = SerializeData(data);

        byte[] bytes = Encoding.UTF8.GetBytes(serializedData);
        byte[] hashBytes = SHA256.HashData(bytes);

        System.Text.StringBuilder builder = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            builder.Append(hashBytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    /// <summary>
    /// Verifies that the hash of the given data matches the expected hash.
    /// </summary>
    /// <param name="data">An object to be hashed.</param>
    /// <param name="expectedHash">The expected hash of the data object.</param>
    /// <returns>True, if the hashes match. Otherwise, false.</returns>
    public bool VerifyHash(T data, string? expectedHash)
    {
        string actualHash = ComputeHash(data);
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Serializes XML data to a string for hashing.
/// </summary>
public class XmlHashProvider : FormatHashProviderBase<IEnumerable<XElement>>
{
    protected override string SerializeData(IEnumerable<XElement> data)
    {
        return string.Join("", data.Select(element => element.ToString()));
    }
}

public class JsonHashProvider : FormatHashProviderBase<IEnumerable<JsonDocument>>
{
    protected override string SerializeData(IEnumerable<JsonDocument> data)
    {
        return string.Join("", data.Select(document => document.RootElement.ToString()));
    }
}