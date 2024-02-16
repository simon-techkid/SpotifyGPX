// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    private static bool ReplaceFiles => false; // Allow SpotifyGPX to replace existing files, rather than generating a unique name
    private readonly IEnumerable<SongPoint> Pairs; // Hold the pairs list that will be exported

    public OutputHandler(IEnumerable<SongPoint> pairs) => Pairs = pairs;

    public void Save(Formats format, string sourceGpxName)
    {
        // file = the output file
        // total = the expected pair count
        // name = the name of the pair batch (usually track name)
        // path = the export path of the final file
        List<(IFileOutput file, int total, string name, string path)> tracksToFiles = new();

        bool supportsMulti = AllowsMultiTrack[format]; // Determine whether the desired format can hold multiple GPX tracks worth of pairs

        if (supportsMulti)
        {
            // If the desired format can support multiple tracks, handle the entire pair list, ungrouped:
            tracksToFiles.Add(HandleTrack(Pairs, format, sourceGpxName, "All"));
        }
        else
        {
            // If the desired format can't support multiple tracks, split each track into its own file:
            foreach (var track in Pairs.GroupBy(pair => pair.Origin)) // Group the pairs by their source GPX track
            {
                tracksToFiles.Add(HandleTrack(track, format, sourceGpxName, track.Key.ToString())); // Original GPX track name included in file name
            }
        }

        tracksToFiles.ForEach(track => track.file.Save(track.path)); // For each track file, save it to the disk

        // Print the individual track results (number of pairs):
        string joinedExports = string.Join(", ", tracksToFiles.Select(track => $"{track.file.Count}/{track.total} ({track.name})"));
        double totalExported = tracksToFiles.Select(track => track.file.Count).Sum(); // Sum of all files' exported pairs
        double totalPairs = tracksToFiles.Select(track => track.total).Sum(); // Sum of all tracks' pairs
        Console.WriteLine($"[OUT] [{format.ToString().ToUpper()} {totalExported}/{totalPairs}]: {joinedExports}");
    }

    private static (IFileOutput, int, string, string) HandleTrack(IEnumerable<SongPoint> pairs, Formats format, string sourceGpxName, string trackName)
    {
        // Include both the original GPX name, and the name of the source GPX track in the output file
        string outputFileName = GetOutputFileName($"{sourceGpxName}_{trackName}", format.ToString().ToLower());
        string path = GetUniqueFilePath(outputFileName); // Ensure exporting to unique file name
        IFileOutput handler = GetHandler(pairs)[format]; // Get file output class with IFileOutput
        return (handler, pairs.Count(), trackName, path); // Return tuple of values for tracksToFiles list
    }

    private static Dictionary<Formats, IFileOutput> GetHandler(IEnumerable<SongPoint> pairs)
    {
        return new Dictionary<Formats, IFileOutput> // Each of the below classes inherit IFileOutput, as they are format classes sharing methods and fields
        {
            { Formats.Gpx, new Gpx(pairs) }, // Initialize a new Gpx class with the pairs list provided
            { Formats.Json, new Json(pairs) },
            { Formats.JsonReport, new JsonReport(pairs) },
            { Formats.Txt, new Txt(pairs) },
            { Formats.Xspf, new Xspf(pairs) }
            // To add a new format, create an entry in enum Formats, and associate it with a class
        };
    }

    private readonly static Dictionary<Formats, bool> AllowsMultiTrack = new()
    {
        { Formats.Gpx, false },
        { Formats.Json, false },
        { Formats.JsonReport, true }, // Supports multiple tracks in the same file
        { Formats.Txt, false },
        { Formats.Xspf, false }
        // To add a new format, create an entry in enum Formats, and define here whether it can hold multiple tracks
    };

    private static string GetOutputFileName(string name, string extension) => $"{name}.{extension}";

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