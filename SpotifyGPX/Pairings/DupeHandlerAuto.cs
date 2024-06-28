// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Pairings;

public class DupeHandlerAuto : DupeHandler
{
    public DupeHandlerAuto(string name, StringBroadcaster bcast) : base(name, bcast) { }

    protected override List<(int, int)> GetDupeIndexes()
    {
        var dupes = GroupDuplicates();

        return dupes.Select(dupe => (dupe.First().Index, dupe.Last().Index)).ToList();
    }
}
