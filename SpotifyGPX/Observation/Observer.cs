// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public abstract class Observer : IObserver<string>
{
    public bool Silent { get; set; }
    protected abstract string CompletionMessage { get; }
    protected virtual bool IgnoreSilent => false;

    public Observer() { }

    public void OnCompleted()
    {
        bool silentState = Silent;
        Silent = false;
        WriteLine(CompletionMessage);
        Silent = silentState;
    }

    public void OnError(Exception error)
    {
        bool silentState = Silent;
        Silent = false;
        WriteLine($"An error occurred: {error.Message}");
        Silent = silentState;
    }

    public void OnNext(string value)
    {
        WriteLine(value);
    }

    protected abstract void HandleMessage(string message);

    private void WriteLine(string value)
    {
        if (!Silent || IgnoreSilent)
        {
            HandleMessage(value);
        }
    }
}
