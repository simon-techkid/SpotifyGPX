// SpotifyGPX by Simon Field

using System;
using System.IO;

namespace SpotifyGPX.Input;

public abstract class FileInputBase : IDisposable
{
    /// <summary>
    /// Serves as the reading stream for a file on the disk.
    /// </summary>
    protected FileStream FileStream { get; private set; }

    /// <summary>
    /// Serves as the stream reader for the file stream, <see cref="FileStream"/>.
    /// </summary>
    protected StreamReader StreamReader { get; private set; }

    /// <summary>
    /// Clears this file's original document contents from memory.
    /// </summary>
    protected abstract void ClearDocument();

    protected FileInputBase(string path)
    {
        FileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        StreamReader = new StreamReader(FileStream);
    }

    public virtual void Dispose()
    {
        StreamReader.Dispose();
        FileStream.Dispose();
        ClearDocument();
        GC.SuppressFinalize(this);
    }
}
