// SpotifyGPX by Simon Field

namespace SpotifyGPX.Broadcasting;

public abstract class StringBroadcasterBase : BroadcasterBase<string>
{
    public override StringBroadcaster BCaster { get; }

    protected StringBroadcasterBase(StringBroadcaster bCaster)
    {
        BCaster = bCaster;
        BCaster.Type = BroadcasterPrefix;
    }

    /// <summary>
    /// The prefix for the broadcaster, displayed as a prefix for all broadcast strings.
    /// </summary>
    protected abstract string BroadcasterPrefix { get; }
}
