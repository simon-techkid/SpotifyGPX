// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for serializing an array of bytes.
/// </summary>
public abstract class ByteSaveable : SaveableBase<byte[], byte[]>
{
    protected ByteSaveable(Func<IEnumerable<SongPoint>> pairs, string? trackName, Broadcaster bcast) : base(pairs, bcast, trackName)
    {
    }

    protected override IHashProvider<byte[]> HashProvider => new ByteHashProvider();

    protected override byte[] ConvertToBytes()
    {
        return Document;
    }

    protected override byte[] ClearDocument()
    {
        return Array.Empty<byte>();
    }
}
