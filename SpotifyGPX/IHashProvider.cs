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
    /// <summary>
    /// Compute a checksum hash for the given <typeparamref name="T"/> data.
    /// </summary>
    /// <param name="data">Data to be hashed, in format <typeparamref name="T"/>.</param>
    /// <returns>A <see langword="string"/> representing a checksum for the given <typeparamref name="T"/> data.</returns>
    string ComputeHash(T data);
}

/// <summary>
/// Unifies all classes supporting verifying a hash of an object with another hash.
/// </summary>
/// <typeparam name="T">The type of the object being hashed.</typeparam>
public interface IHashChecker<T>
{
    /// <summary>
    /// Verify that the hash of the given <typeparamref name="T"/> data matches the expected hash.
    /// </summary>
    /// <param name="data">Data to be hashed, in format <typeparamref name="T"/>.</param>
    /// <param name="expectedHash">A <see langword="string"/> representing the expected hash for the given <typeparamref name="T"/> data.</param>
    /// <returns>True, if the calculated hash of the <typeparamref name="T"/> data matches the given expected hash. Otherwise, false.</returns>
    bool VerifyHash(T data, string expectedHash);
}

/// <summary>
/// Central class supporting creating and verifying hashes for documents of type <typeparamref name="T"/>.
/// The document is converted to a <see langword="byte"/>[] before hashing.
/// </summary>
/// <typeparam name="T">The type of the object being hashed.</typeparam>
public abstract class ByteFormatHashProviderBase<T> : IHashProvider<T>, IHashChecker<T>
{
    /// <summary>
    /// Convert an object of type <typeparamref name="T"/> to a <see langword="byte"/>[] so that the byte array can serve as the hashed payload.
    /// </summary>
    /// <param name="data">An object of type <typeparamref name="T"/> to be converted to <see langword="byte"/>[].</param>
    /// <returns>This document of type <typeparamref name="T"/> as a <see langword="byte"/>[].</returns>
    protected abstract byte[] ConvertToBytes(T data);

    public string ComputeHash(T data)
    {
        byte[] bytes = ConvertToBytes(data);
        byte[] hashBytes = SHA256.HashData(bytes);

        System.Text.StringBuilder builder = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            builder.Append(hashBytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    public bool VerifyHash(T data, string? expectedHash)
    {
        string actualHash = ComputeHash(data);
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Central class supporting stringifying objects of type <typeparamref name="T"/> prior to being hashed.
/// The document is converted to a <see langword="string"/> before it is converted to a <see langword="byte"/>[] and hashed.
/// </summary>
/// <typeparam name="T">The type of the object being hashed.</typeparam>
public abstract class StringFormatHashProviderBase<T> : ByteFormatHashProviderBase<T>
{
    protected StringFormatHashProviderBase(Encoding? encoding = null)
    {
        HashedDataEncoding = encoding ?? Encoding.UTF8;
    }

    /// <summary>
    /// Convert an object of type <typeparamref name="T"/> to a <see langword="string"/> so that the string can serve as the hashed payload.
    /// </summary>
    /// <param name="data">An object of type <typeparamref name="T"/> to be hashed.</param>
    /// <returns>The object as a string.</returns>
    protected abstract string ConvertToString(T data);

    /// <summary>
    /// The encoding of the document of type <typeparamref name="T"/> as a string to be hashed.
    /// Default value: <see cref="Encoding.UTF8"/>, override to change.
    /// </summary>
    protected virtual Encoding HashedDataEncoding { get; }

    protected override byte[] ConvertToBytes(T data)
    {
        string serializedData = ConvertToString(data);
        return HashedDataEncoding.GetBytes(serializedData);
    }
}

public class XmlHashProvider : StringFormatHashProviderBase<IEnumerable<XElement>>
{
    public XmlHashProvider(Encoding? encoding = null) : base(encoding)
    {
    }

    protected override string ConvertToString(IEnumerable<XElement> data)
    {
        return string.Join("", data.Select(data => data.ToString()));
    }
}

public class JsonHashProvider : StringFormatHashProviderBase<List<JsonDocument>>
{
    public JsonHashProvider(Encoding? encoding = null) : base(encoding)
    {
    }

    protected override string ConvertToString(List<JsonDocument> data)
    {
        return string.Join("", data.Select(document => document.RootElement.ToString()));
    }
}

public class TxtHashProvider : StringFormatHashProviderBase<string?[]>
{
    public TxtHashProvider(Encoding? encoding = null) : base(encoding)
    {
    }

    protected override string ConvertToString(string?[] data)
    {
        return string.Join("", data);
    }
}

public class ByteHashProvider : ByteFormatHashProviderBase<byte[]>
{
    protected override byte[] ConvertToBytes(byte[] data)
    {
        return data;
    }
}