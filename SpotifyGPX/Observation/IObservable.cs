// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public interface IObservable<out T>
{
    /// <summary>
    /// Subscribe an observer of type <typeparamref name="T"/> to the broadcaster of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="observer">An observer of type <typeparamref name="T"/> that is <see cref="Observation.IObserver{T}"/> and matches the type (<typeparamref name="T"/>) of this broadcaster.</param>
    /// <returns>An <see cref="IDisposable"/> disposer that, when called, unsubscribes the observer from this broadcaster.</returns>
    IDisposable Subscribe(IObserver<T> observer);

    /// <summary>
    /// The hash code of the broadcaster of type <typeparamref name="T"/>.
    /// </summary>
    int HashCode { get; }
}
