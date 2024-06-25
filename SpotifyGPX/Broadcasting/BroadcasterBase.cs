// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Broadcasting;

public abstract class BroadcasterBase<T> : Observation.IObservable<T>
{
    /// <summary>
    /// The broadcaster of type <typeparamref name="T"/> that is used to broadcast messages of type <typeparamref name="T"/>.
    /// </summary>
    public abstract Broadcaster<T> BCaster { get; }

    public int HashCode => BCaster.HashCode;

    public IDisposable Subscribe(Observation.IObserver<T> observer)
    {
        return BCaster.Subscribe(observer);
    }
}
