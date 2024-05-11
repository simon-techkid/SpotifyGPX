// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;

namespace SpotifyGPX.Output;

public partial class FileOutputFactory
{
    private delegate IFileOutput FileOutputCreator(Func<IEnumerable<SongPoint>> pairs, string trackName);

    public IFileOutput CreateFileOutput(Formats format, Func<IEnumerable<SongPoint>> pairs, string trackName)
    {
        if (creators.TryGetValue(format, out FileOutputCreator? creator) && creator != null)
        {
            return creator.Invoke(pairs, trackName);
        }
        else
        {
            throw new Exception($"Unsupported file export format: {format}");
        }
    }
}

