using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    private static bool ReplaceDuplicateFiles => true;
    private string GetOutputFileName(string name) => $"{name}.{TargetFormat.ToString().ToLower()}";

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

    private double SaveFile(IEnumerable<SongPoint> pairs, string path)
    {
        IFileOutput export = GetFileSaver(pairs);
        export.Save(path);

        return export.Count;
    }

    public void Save(string name)
    {
        if (SupportsMultiTrack)
        {
            string fileName = GetUniqueFileName(GetOutputFileName(name));
            double count = SaveFile(Pairs, fileName);
        }
        else
        {
            List<(double count, TrackInfo track)> trackReturns = Pairs
            .GroupBy(pair => pair.Origin)
            .Select(track =>
            {
                TrackInfo currentTrack = track.Key;
                string fileName = GetUniqueFileName(GetOutputFileName($"{name}_{currentTrack}"));
                double count = SaveFile(track, fileName);
                return (count, currentTrack);
            })
            .ToList();

            string joinedValues = string.Join(", ", trackReturns.Select(tuple => $"{tuple.count} ({tuple.track.ToString()})"));
            Console.WriteLine($"[OUT] Exporting to {TargetFormat}: {joinedValues}");
        }
    }

    private static string GetUniqueFileName(string path)
    {
        if (!File.Exists(path) || ReplaceDuplicateFiles)
        {
            return path; // If the file doesn't exist, return the original name
        }

        string directory = Path.GetDirectoryName(path);
        string fileExtension = Path.GetExtension(path);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

        int count = 1;
        string uniqueFileName = path;

        do
        {
            uniqueFileName = Path.Combine(directory, $"{fileNameWithoutExtension}_{count}{fileExtension}");
            count++;
        } while (File.Exists(uniqueFileName));

        return uniqueFileName;
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