// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;

namespace SpotifyGPX;

public abstract class DisposableBase : StringBroadcasterBase, IDisposable
{
    protected DisposableBase(StringBroadcaster bcast) : base(bcast) { }

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
        BCaster.Broadcast("Object disposed", Observation.LogLevel.Debug);
        GC.SuppressFinalize(this);
    }
}
