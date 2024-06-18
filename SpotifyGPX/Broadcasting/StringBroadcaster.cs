// SpotifyGPX by Simon Field

namespace SpotifyGPX.Broadcasting;

public class StringBroadcaster : Broadcaster<string>
{
    public string Type { get; set; } = "INFO";

    protected override string BroadcastHandler(string message)
    {
        return $"[{Type}] {message}";
    }
}
