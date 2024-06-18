// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public interface IObservable<out T>
{
    IDisposable Subscribe(IObserver<T> observer);
}
