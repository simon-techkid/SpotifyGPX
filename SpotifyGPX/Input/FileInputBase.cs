// SpotifyGPX by Simon Field

using System.IO;

namespace SpotifyGPX.Input;

public abstract class FileInputBase : DisposableBase
{
    protected FileInputBase(string path)
    {
        FileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        StreamReader = new StreamReader(FileStream);
    }

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
    protected abstract void DisposeDocument();

    protected override void DisposeClass()
    {
        StreamReader.Dispose();
        FileStream.Dispose();
        DisposeDocument();
    }
}
