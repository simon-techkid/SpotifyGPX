// SpotifyGPX by Simon Field

using System.Collections.Generic;

namespace SpotifyGPX.Input;

public interface IPairInput
{
    public List<SongPoint> GetAllPairs();
    int PairCount { get; }
}
