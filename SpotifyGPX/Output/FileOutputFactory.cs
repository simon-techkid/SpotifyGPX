using System;
using System.Collections.Generic;

namespace SpotifyGPX.Output;

public class FileOutputFactory
{
    private delegate IFileOutput FileOutputCreator(Func<IEnumerable<SongPoint>> pairs, string trackName);

    private readonly Dictionary<Formats, FileOutputCreator> creators = new()
    {
        { Formats.Csv, (pairs, trackName) => new Csv(pairs, trackName) },
        { Formats.Gpx, (pairs, trackName) => new Gpx(pairs, trackName) },
        { Formats.Json, (pairs, trackName) => new Json(pairs, trackName) },
        { Formats.JsonReport, (pairs, trackName) => new JsonReport(pairs, trackName) },
        { Formats.Kml, (pairs, trackName) => new Kml(pairs, trackName) },
        { Formats.Txt, (pairs, trackName) => new Txt(pairs, trackName) },
        { Formats.Xspf, (pairs, trackName) => new Xspf(pairs, trackName) }
    };

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

