// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for serializing <typeparamref name="TDocument"/> data to <see langword="byte"/>[] and saving it in the target format.
/// </summary>
/// <typeparam name="TDocument">The source format type.</typeparam>
/// <typeparam name="THashed">The format type of the hashable portion of this document.</typeparam>
public abstract class SaveableBase<TDocument, THashed> : DisposableBase, IFileOutput
{
    protected SaveableBase(Func<IEnumerable<SongPoint>> pairs, string? trackName)
    {
        DataProvider = pairs;
        Document = SaveAction(trackName);
    }

    public abstract string FormatName { get; }

    /// <summary>
    /// The document in format <typeparamref name="TDocument"/> that will be serialized and saved to the disk.
    /// </summary>
    protected TDocument Document { get; private set; }
    public abstract int Count { get; }

    /// <summary>
    /// Provides access to the collection of <see cref="SongPoint"/> pairs to be saved to the document in format <typeparamref name="TDocument"/>.
    /// </summary>
    protected Func<IEnumerable<SongPoint>> DataProvider { get; }

    /// <summary>
    /// A delegate to access the contents of the document in format <typeparamref name="TDocument"/>.
    /// </summary>
    /// <returns></returns>
    protected delegate TDocument DocumentAccessor(string? trackName);

    /// <summary>
    /// Provides access to the document in format <typeparamref name="TDocument"/> that will be serialized and saved to the disk.
    /// </summary>
    protected abstract DocumentAccessor SaveAction { get; }

    public void Save(string path)
    {
        byte[] doc = ConvertToBytes();
        Save(path, doc);
    }

    protected virtual void Save(string path, byte[] bytes)
    {
        using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);
        fileStream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Converts <typeparamref name="TDocument"/> to <see langword="byte"/>[].
    /// </summary>
    /// <returns>This <see cref="Document"/>, as <see langword="byte"/>[].</returns>
    protected abstract byte[] ConvertToBytes();

    /// <summary>
    /// The <see cref="IHashProvider{T}"/> for generating a hash of the document of type <typeparamref name="THashed"/>
    /// </summary>
    protected abstract IHashProvider<THashed> HashProvider { get; }

    /// <summary>
    /// Clears the contents of the <see cref="Document"/> in preparation for disposal.
    /// </summary>
    /// <returns>A <typeparamref name="TDocument"/> that has been cleared.</returns>
    protected abstract TDocument ClearDocument();

    protected override void DisposeClass()
    {
        Document = ClearDocument();
    }
}
