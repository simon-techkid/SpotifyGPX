// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using SpotifyGPX.Input;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX;

public class PairingsFromPairings : PairingsFactory
{
    private string InputPairs { get; }

    public PairingsFromPairings(string inputPairs, StringBroadcaster bcast, bool predict, bool autoPredict) : base(bcast, predict, autoPredict)
    {
        InputPairs = inputPairs;
    }

    protected override string PairingsSetName => Path.GetFileNameWithoutExtension(InputPairs);

    public override PairingsHandler GetHandler()
    {
        PairingsHandler pairer = GetDupeOrRegHandler();

        List<SongPoint> pairs = new();

        using (InputHandler input = new(InputPairs, BCaster.Clone()))
        {
            pairs = input.GetAllPairs();
        }

        pairer.CalculatePairings(pairs);
        return pairer;
    }
}
