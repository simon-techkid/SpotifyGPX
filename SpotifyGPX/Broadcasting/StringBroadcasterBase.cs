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

    protected abstract string BroadcasterPrefix { get; }
}
