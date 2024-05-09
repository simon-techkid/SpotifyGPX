// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public class ConsoleObserver : Observer
{
    public ConsoleObserver() : base() { }
    protected override string CompletionMessage => "Job completed.";

    protected override void HandleMessage(string message)
    {
        Console.WriteLine(message);
    }
}
