// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Handle various output file formats for exporting pairs
/// </summary>
public class OutputHandler
{
    private static bool ReplaceFiles => true; // Allow SpotifyGPX to replace existing files, rather than generating a unique name
    private IEnumerable<SongPoint> Pairs { get; } // Hold the pairs list that will be exported

    /// <summary>
    /// Creates a handler for exporting output files
    /// </summary>
    /// <param name="pairs">A list of pairs to send to a file.</param>
    public OutputHandler(IEnumerable<SongPoint> pairs) => Pairs = pairs;

    /// <summary>
    /// Saves the pairs contained in this handler to a file in the given format.
    /// </summary>
    /// <param name="format">The format to use for the exported file.</param>
    /// <param name="sourceGpxName">The name of the original GPS file used.</param>
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

    /// <summary>
    /// Represents an output file.
    /// </summary>
    private readonly struct OutFile
    {
        /// <summary>
        /// Stages an output file, containing the given pairs, in the given format, with the specified names.
        /// </summary>
        /// <param name="pairs">The pairs to include in the output file.</param>
        /// <param name="format">The format of the output file.</param>
        /// <param name="sourceGpxName">The prefix of the output file name.</param>
        /// <param name="trackName">The name of the track represented in this file (if this file only has one track).</param>
        public OutFile(IEnumerable<SongPoint> pairs, Formats format, string sourceGpxName, string trackName)
        {
            Handler = CreateFileOutput(format, pairs, trackName);
            OriginalCount = pairs.Count();
            Name = trackName;
            string fileName = $"{sourceGpxName}_{trackName}";
            string extension = format.ToString().ToLower();
            Path = GetUniqueFilePath($"{fileName}.{extension}"); // Ensure exporting to unique file name
        }

        /// <summary>
        /// The output file handler.
        /// </summary>
        private IFileOutput Handler { get; }

        /// <summary>
        /// The number of pairs in the original pairing list
        /// </summary>
        public int OriginalCount { get; }

        /// <summary>
        /// The number of pairs apart of the exported file
        /// </summary>
        public int ExportCount => Handler.Count;

        /// <summary>
        /// The name of the track
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// The export path of the final file
        /// </summary>
        private string Path { get; }

        /// <summary>
        /// A string representing the outcome of the export.
        /// </summary>
        public string Result => $"{ExportCount}/{OriginalCount} ({Name})";

        /// <summary>
        /// Save the file to the path.
        /// </summary>
        public void Save() => Handler.Save(Path);
    }

    /// <summary>
    /// Determines the appropriate output class for handling pairs in the given format.
    /// </summary>
    /// <param name="format">The desired export format.</param>
    /// <param name="pairs">The pairs to create in the specified file format.</param>
    /// <returns>An IFileOutput interface allowing interfacing with the corresponding format.</returns>
    /// <exception cref="Exception">The provided file doesn't have an export class associated with it.</exception>
    private static IFileOutput CreateFileOutput(Formats format, IEnumerable<SongPoint> pairs, string trackName)
    {
        return format switch
        {
            Formats.Csv => new Csv(pairs),
            Formats.Gpx => new Gpx(pairs, trackName),
            Formats.Html => new Html(pairs, trackName),
            Formats.Json => new Json(pairs),
            Formats.JsonReport => new JsonReport(pairs),
            Formats.Txt => new Txt(pairs),
            Formats.Xspf => new Xspf(pairs, trackName),
            _ => throw new Exception($"Unsupported file export format: {format}")
        };
    }

    /// <summary>
    /// Determines whether the given format supports multiple tracks (distinguishing between them).
    /// </summary>
    /// <param name="format">The format.</param>
    /// <returns>True, if the format supports multiple tracks. If it doesn't, false.</returns>
    /// <exception cref="Exception"></exception>
    private static bool AllowsMultiTrack(Formats format)
    {
        return format switch
        {
            Formats.Csv => false,
            Formats.Gpx => false,
            Formats.Html => false,
            Formats.Json => false,
            Formats.JsonReport => true,
            Formats.Txt => false,
            Formats.Xspf => false,
            _ => throw new Exception($"Unsupported file export format: {format}")
        };
    }

    /// <summary>
    /// Generate a unique file path given a provided path.
    /// </summary>
    /// <param name="path">A path that will be checked for an existing file.</param>
    /// <returns>If the given path is already an existing file, a unique path that doesn't already exist as a file. Otherwise, the original path.</returns>
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

/// <summary>
/// A list of the supported export formats.
/// </summary>
public enum Formats
{
    /// <summary>
    /// A CSV file containing a pair per line.
    /// </summary>
    Csv,

    /// <summary>
    /// A GPX file containing song-point pairs as a waypoints.
    /// </summary>
    Gpx,

    /// <summary>
    /// A HTML file containing a visualized webpage of pairs.
    /// </summary>
    Html,

    /// <summary>
    /// A JSON file containing only the original Spotify data records used for pairs.
    /// </summary>
    Json,

    /// <summary>
    /// A .jsonreport file containing all pairing and track data.
    /// </summary>
    JsonReport,

    /// <summary>
    /// A plain text file containing a string per pair.
    /// </summary>
    Txt,

    /// <summary>
    /// An XML playlist file compatible with audio playback software.
    /// </summary>
    Xspf
}

/// <summary>
/// Interfaces with file output classes, unifying all formats that pairs can written to.
/// </summary>
public interface IFileOutput
{
    // Defines the requirements of export format classes:
    void Save(string path); // Allows the saving of that file to the local disk
    int Count { get; } // Provides the number of pairings in the file
}