using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public enum Formats
{
    Gpx,
    Json,
    JsonReport,
    Txt,
    Xspf
}

public class FormatHandler
{
    public FormatHandler(IEnumerable<SongPoint> pairs, Formats format)
    {
        Pairs = pairs;
        TargetFormat = format;
    }

    private IEnumerable<SongPoint> Pairs { get; }

    private Formats TargetFormat { get; }

    public void Save(string prefix)
    {
        if (SupportsMultiTrack)
        {
            string fileName = $"{prefix}_S.{TargetFormat.ToString().ToLower()}";
            SaveFile(Pairs, fileName);
        }
        else
        {
            foreach (var track in Pairs.GroupBy(pair => pair.Origin))
            {
                TrackInfo currentTrack = track.Key;

                string fileName = $"{prefix}_{currentTrack.ToString()}_S.{TargetFormat.ToString().ToLower()}";
                SaveFile(track, fileName);
            }
        }
    }

    private bool SupportsMultiTrack => TargetFormat switch
    {
        Formats.Gpx => Gpx.SupportsMultiTrack,
        Formats.Json => Json.SupportsMultiTrack,
        Formats.JsonReport => JsonReport.SupportsMultiTrack,
        Formats.Txt => Txt.SupportsMultiTrack,
        Formats.Xspf => Xspf.SupportsMultiTrack,
        _ => throw new Exception("Invalid Format specified when saving pairings!")
    };

    public void SaveFile(IEnumerable<SongPoint> pairs, string path)
    {
        IFileOutput export = GetFileSaver(pairs);
        export.Save(path);

        double percentSuccess = (export.Count / pairs.Count()) * 100;
        Console.WriteLine($"[FILE] {Path.GetFileName(path)}: {export.Count}/{pairs.Count()} ({percentSuccess}%)");

    }

    private IFileOutput GetFileSaver(IEnumerable<SongPoint> pairs)
    {
        return TargetFormat switch
        {
            Formats.Gpx => new Gpx(pairs),
            Formats.Json => new Json(pairs),
            Formats.JsonReport => new JsonReport(pairs),
            Formats.Txt => new Txt(pairs),
            Formats.Xspf => new Xspf(pairs),
            _ => throw new Exception("Invalid Format specified when saving pairings!")
        };
    }

    public interface IFileOutput
    {
        int Count { get; }
        void Save(string path);
    }
}