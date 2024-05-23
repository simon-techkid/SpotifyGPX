// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

public abstract class DisposableBase : IDisposable
{
    /// <summary>
    /// Indicates whether the class has been disposed of.
    /// </summary>
    public bool Disposed { get; protected set; }

    /// <summary>
    /// Dispose of the class's resources.
    /// </summary>
    protected abstract void DisposeClass();

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        DisposeClass();
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
