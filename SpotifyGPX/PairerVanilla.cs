// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;

namespace SpotifyGPX;

public class PairerVanilla : PairingsHandler
{
    public override string Name { get; }

    public PairerVanilla(string name, StringBroadcaster bcast) : base(bcast)
    {
        Name = name;
    }
}
