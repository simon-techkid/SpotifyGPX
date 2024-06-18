// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Observation;

public partial class ConsoleObserver : Observer<string>
{
    public ConsoleObserver(LogLevel level) : base()
    {
        Level = level;
    }

    public override LogLevel Level { get; }

    protected override void HandleMessage(string message)
    {
        Console.WriteLine(message);
    }

    protected override void HandleException(Exception exception)
    {
        Console.WriteLine(exception.ToString());
    }

    protected override void HandleCompletion()
    {
        Console.WriteLine("Job completed.");
    }
}
