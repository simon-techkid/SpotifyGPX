// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Broadcasting;

public abstract class BroadcasterBase : IObservable<string>
{
    public Broadcaster BCaster { get; }

    protected BroadcasterBase(Broadcaster bCaster)
    {
        BCaster = bCaster;
        BCaster.Type = BroadcasterPrefix;
    }

    protected abstract string BroadcasterPrefix { get; }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        return BCaster.Subscribe(observer);
    }
}
