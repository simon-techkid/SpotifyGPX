// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;

namespace SpotifyGPX.Broadcasting;

public class Broadcaster
{
    public List<IObserver<string>> Observers { get; set; }

    public string Type { get; set; }

    public Broadcaster()
    {
        Type = "INFO";
        Observers = new();
    }

    public void Broadcast(string message)
    {
        foreach (var observer in Observers)
        {
            observer.OnNext($"[{Type}] {message}");
        }
    }

    public void BroadcastError(Exception error)
    {
        foreach (var observer in Observers)
        {
            observer.OnError(error);
        }
    }

    public void BroadcastCompletion()
    {
        foreach (var observer in Observers)
        {
            observer.OnCompleted();
        }
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        if (!Observers.Contains(observer))
        {
            Observers.Add(observer);
        }

        IDisposable? additional = observer as IDisposable ?? null;

        return new Unsubscriber(observer, Observers, additional);
    }

    private class Unsubscriber : IDisposable
    {
        private IObserver<string> _observer;
        private List<IObserver<string>> _observers;
        private IDisposable? _additional;

        public Unsubscriber(IObserver<string> observer, List<IObserver<string>> observers, IDisposable? additional)
        {
            _observer = observer;
            _observers = observers;
            _additional = additional;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _additional?.Dispose();
                _observers.Remove(_observer);
            }
        }
    }
}
