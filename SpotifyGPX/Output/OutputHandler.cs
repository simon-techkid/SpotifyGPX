using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    public OutputHandler(IEnumerable<SongPoint> pairs, Formats format)
    {
        Pairs = pairs;
        TargetFormat = format;
    }

    private IEnumerable<SongPoint> Pairs { get; }

    private Formats TargetFormat { get; }


    private static readonly Dictionary<Formats, Type> FormatTypeMapping = new()
    {
        { Formats.Gpx, typeof(Gpx) },
        { Formats.Json, typeof(Json) },
        { Formats.JsonReport, typeof(JsonReport) },
        { Formats.Txt, typeof(Txt) },
        { Formats.Xspf, typeof(Xspf) }
    };

    private IFileOutput GetFileSaver(IEnumerable<SongPoint> pairs)
    {
        if (FormatTypeMapping.TryGetValue(TargetFormat, out Type fileType))
        {
            return Activator.CreateInstance(fileType, pairs) as IFileOutput;
        }
        else
        {
            throw new Exception("Invalid Format specified when saving pairings!");
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

    private void SaveFile(IEnumerable<SongPoint> pairs, string path)
    {
        IFileOutput export = GetFileSaver(pairs);
        export.Save(path);

        double percentSuccess = (export.Count / pairs.Count()) * 100;
        Console.WriteLine($"[FILE] {Path.GetFileName(path)}: {export.Count}/{pairs.Count()} ({percentSuccess}%)");

    }

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

    public interface IFileOutput
    {
        int Count { get; }
        void Save(string path);
    }

    public enum Formats
    {
        Gpx,
        Json,
        JsonReport,
        Txt,
        Xspf
    }
}