using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    private static bool ReplaceFiles => false;
    private readonly IEnumerable<SongPoint> Pairs;

    public OutputHandler(IEnumerable<SongPoint> pairs) => Pairs = pairs;

    public void Save(string sourceGpxName, Formats format)
    {
        string lowerFormat = format.ToString().ToLower(); // All lowercase format name
        string upperFormat = format.ToString().ToUpper(); // All uppercase format name

        // file = the output file
        // total = the expected pair count
        // name = the name of the pair batch (usually track name)
        // path = the export path of the final file
        List<(IFileOutput file, int total, string name, string path)> tracksToFiles = new();

        bool supportsMulti = AllowsMultiTrack()[format]; // Determine whether the desired format can hold multiple GPX tracks worth of pairs

        if (supportsMulti)
        {
            tracksToFiles.Add(HandleTrack(Pairs, format, sourceGpxName, "All"));
        }
        else
        {
            var tracks = Pairs.GroupBy(pair => pair.Origin); // Group the pairs by their source GPX track

            foreach (var track in tracks)
            {
                tracksToFiles.Add(HandleTrack(track, format, sourceGpxName, track.Key.ToString()));
            }
        }

        tracksToFiles.ForEach(track => track.file.Save(track.path));

        string joinedExports = string.Join(", ", tracksToFiles.Select(track => $"{track.file.Count}/{track.total} ({track.name})"));
        double totalExported = tracksToFiles.Select(track => track.file.Count).Sum();
        double totalPairs = tracksToFiles.Select(track => track.total).Sum();
        Console.WriteLine($"[OUT] [{upperFormat} {totalExported}/{totalPairs}]: {joinedExports}");
    }

    private static (IFileOutput, int, string, string) HandleTrack(IEnumerable<SongPoint> pairs, Formats format, string sourceGpxName, string trackName)
    {
        string outputFileName = GetOutputFileName($"{sourceGpxName}_{trackName}", format.ToString().ToLower());
        string path = GetUniqueFilePath(outputFileName);
        IFileOutput handler = GetHandler(pairs)[format];
        return (handler, pairs.Count(), trackName, path);
    }

    private static Dictionary<Formats, IFileOutput> GetHandler(IEnumerable<SongPoint> pairs)
    {
        return new Dictionary<Formats, IFileOutput>
        {
            { Formats.Gpx, new Gpx(pairs) },
            { Formats.Json, new Json(pairs) },
            { Formats.JsonReport, new JsonReport(pairs) },
            { Formats.Txt, new Txt(pairs) },
            { Formats.Xspf, new Xspf(pairs) }
        };
    }

    private static Dictionary<Formats, bool> AllowsMultiTrack()
    {
        return new Dictionary<Formats, bool>
        {
            { Formats.Gpx, false },
            { Formats.Json, false },
            { Formats.JsonReport, true },
            { Formats.Txt, false },
            { Formats.Xspf, false }
        };
    }

    private static string GetOutputFileName(string name, string format) => $"{name}.{format}";

    private static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path) || ReplaceFiles)
        {
            // File doesn't exist, return the provided path
            // Or, replacing files permitted, return the provided path
            return path;
        }

        // Replacing existing files not allowed, generate a unique name:

        string? directory = Path.GetDirectoryName(path);
        string fileExtension = Path.GetExtension(path);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

        int count = 1; // First suffix iteration number
        string uniqueFileName;

        do
        {
            // Unique file placed in directory of original file path, with numeric suffix to distinguish
            uniqueFileName = Path.Combine(directory ?? string.Empty, $"{fileNameWithoutExtension}_{count}{fileExtension}");
            count++;
        } while (File.Exists(uniqueFileName)); // Until non-existing file path found, repeat iteration

        return uniqueFileName;
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

public interface IFileOutput
{
    // Defines the requirements of export format classes:
    void Save(string path); // Allows the saving of that file to the local disk
    int Count { get; } // Provides the number of pairings in the file
}