// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    private static bool ReplaceFiles => false; // Allow SpotifyGPX to replace existing files, rather than generating a unique name
    private IEnumerable<SongPoint> Pairs { get; } // Hold the pairs list that will be exported

    public OutputHandler(IEnumerable<SongPoint> pairs) => Pairs = pairs;

    public void Save(Formats format, string sourceGpxName)
    {
        List<OutFile> files = new();

        bool supportsMulti = AllowsMultiTrack(format); // Determine whether the desired format can hold multiple GPX tracks worth of pairs

        if (supportsMulti)
        {
            // If the desired format supports multiple tracks, provide the entire pair list:
            files.Add(new OutFile(Pairs, format, sourceGpxName, "All"));
        }
        else
        {
            // If the desired format doesn't support multiple tracks, split each track into its own file:
            files
                .AddRange(Pairs
                .GroupBy(pair => pair.Origin) // One track per file
                .Select(track => new OutFile(track, format, sourceGpxName, track.Key.ToString())));
        }

        files.ForEach(file => file.Save()); // Save each file to the disk

        // Print the individual track results (number of pairs):
        string joinedExports = string.Join(", ", files.Select(file => file.Result));
        double totalExported = files.Select(file => file.ExportCount).Sum(); // Sum of all files' exported pairs
        double totalPairs = files.Select(file => file.OriginalCount).Sum(); // Sum of all tracks' pairs
        Console.WriteLine($"[OUT] [{format.ToString().ToUpper()} {totalExported}/{totalPairs}]: {joinedExports}");
    }

    private readonly struct OutFile
    {
        public OutFile(IEnumerable<SongPoint> pairs, Formats format, string sourceGpxName, string trackName)
        {
            Handler = CreateFileOutput(format, pairs);
            OriginalCount = pairs.Count();
            Name = trackName;
            string fileName = $"{sourceGpxName}_{trackName}";
            string extension = format.ToString().ToLower();
            Path = GetUniqueFilePath($"{fileName}.{extension}"); // Ensure exporting to unique file name
        }

        private IFileOutput Handler { get; } // The output file
        public int OriginalCount { get; } // The number of pairs in the original pairing list
        public int ExportCount => Handler.Count; // The number of pairs apart of the exported file
        private string Name { get; } // The name of the track
        private string Path { get; } // The export path of the final file
        public string Result => $"{ExportCount}/{OriginalCount} ({Name})"; // Printed string of outcome
        public void Save() => Handler.Save(Path); // Save the file to the path
    }

    private static IFileOutput CreateFileOutput(Formats format, IEnumerable<SongPoint> pairs)
    {
        return format switch
        {
            Formats.Gpx => new Gpx(pairs),
            Formats.Json => new Json(pairs),
            Formats.JsonReport => new JsonReport(pairs),
            Formats.Txt => new Txt(pairs),
            Formats.Xspf => new Xspf(pairs),
            _ => throw new Exception($"Unsupported file export format: {format}")
        };
    }

    private static bool AllowsMultiTrack(Formats format)
    {
        return format switch
        {
            Formats.Gpx => false,
            Formats.Json => false,
            Formats.JsonReport => true,
            Formats.Txt => false,
            Formats.Xspf => false,
            _ => throw new Exception($"Unsupported file export format: {format}")
        };
    }

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
}

public enum Formats
{
    Gpx,
    Json,
    JsonReport,
    Txt,
    Xspf
}

public interface IFileOutput
{
    // Defines the requirements of export format classes:
    void Save(string path); // Allows the saving of that file to the local disk
    int Count { get; } // Provides the number of pairings in the file
}