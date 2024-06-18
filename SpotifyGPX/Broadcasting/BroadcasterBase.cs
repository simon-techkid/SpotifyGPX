// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Broadcasting;

public abstract class BroadcasterBase<T> : Observation.IObservable<T>
{
    /// <summary>
    /// The broadcaster of type <typeparamref name="T"/> that is used to broadcast messages of type <typeparamref name="T"/>.
    /// </summary>
    public abstract Broadcaster<T> BCaster { get; }

    /// <summary>
    /// Subscribe an observer of type <typeparamref name="T"/> to the broadcaster of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="observer">An observer of type <typeparamref name="T"/> that is <see cref="Observation.IObserver{T}"/> and matches the type (<typeparamref name="T"/>) of this broadcaster.</param>
    /// <returns>An <see cref="IDisposable"/> disposer that, when called, unsubscribes the observer from this broadcaster.</returns>
    public IDisposable Subscribe(Observation.IObserver<T> observer)
    {
        return BCaster.Subscribe(observer);
    }
}
