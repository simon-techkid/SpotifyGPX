// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public interface IObserver<in T>
{
    void OnCompleted();
    void OnError(Exception error);
    void OnNext(T value);
    LogLevel Level { get; }
    bool MessageMatcher(LogLevel lvl);
}
