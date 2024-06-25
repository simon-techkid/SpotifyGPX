// SpotifyGPX by Simon Field

namespace SpotifyGPX.Broadcasting;

public class StringBroadcaster : Broadcaster<string>
{
    public string Type { get; set; } = "INFO";

    protected override string BroadcastHandler(string message)
    {
        return $"[{Type}] {message}";
        //return $"[{HashCode}] [{Type}] {message}";
        // optionally, show the hash code of the broadcaster
        // When observers handle LogLevel, allow Debug observer to see hash codes
    }

    protected override void AdditionalSubscriptionInstructions()
    {
        Broadcast($"New subscriber to broadcaster ({HashCode}), now with {Observers.Count} observers.", Observation.LogLevel.Debug);
    }

    public override StringBroadcaster Clone()
    {
        StringBroadcaster clone = new()
        {
            Observers = Observers
        };

        Broadcast($"Cloned broadcaster ({HashCode}) with {Observers.Count} observers into new broadcaster ({clone.HashCode}) with {clone.Observers.Count} observers.", Observation.LogLevel.Debug);

        return clone;
    }
}
