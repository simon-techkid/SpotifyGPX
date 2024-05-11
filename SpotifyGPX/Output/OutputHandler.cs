// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SpotifyGPX.Output;

/// <summary>
/// Handle various output file formats for exporting pairs
/// </summary>
public partial class OutputHandler
{
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
    /// <param name="name">The name of the original GPS file used.</param>
    /// <param name="transform">Transform the created files based on XSLT stylesheets.</param>
    public void Save(Formats format, string name, bool transform)
    {
        List<OutFile> files = new();

        bool supportsMulti = AllowsMultiTrack(format); // Determine whether the desired format can hold multiple GPX tracks worth of pairs

        if (supportsMulti)
        {
            // If the desired format supports multiple tracks, provide the entire pair list:
            files.Add(new OutFile(Pairs, format, name, AllTracksName));
        }
        else
        {
            // If the desired format doesn't support multiple tracks, split each track into its own file:
            files
                .AddRange(Pairs.GroupBy(pair => pair.Origin) // One track per file
                .Select(track => new OutFile(track, format, name, track.Key.ToString())));
        }

        files.ForEach(file => file.Save(transform)); // Save each file to the disk

        // Print the individual track results (number of pairs):
        LogExportResults(files, format);
    }

    private static void LogExportResults(List<OutFile> files, Formats format)
    {
        string joinedExports = string.Join(", ", files.Select(file => file.Result));
        int totalExported = files.Sum(file => file.ExportCount);
        int totalPairs = files.Sum(file => file.OriginalCount);
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
        /// <param name="name">The prefix of the output file name.</param>
        /// <param name="trackName">The name of the track represented in this file (if this file only has one track).</param>
        public OutFile(IEnumerable<SongPoint> pairs, Formats format, string name, string trackName)
        {
            var factory = new FileOutputFactory();
            Handler = factory.CreateFileOutput(format, () => pairs, trackName);
            SourceName = name;
            TrackName = trackName;
            OriginalCount = pairs.Count();
        }

        private IFileOutput Handler { get; }
        private string SourceName { get; }
        private string TrackName { get; }
        public int OriginalCount { get; }
        public int ExportCount => Handler.Count;
        private string FinalName => $"{SourceName}_{TrackName}.{Handler.FormatName}";
        public string Result => $"{ExportCount}/{OriginalCount} ({TrackName})";

        public void Save(bool transform)
        {
            AttemptSave(transform, FinalName);
        }

        private void AttemptSave(bool transform, string fileName, int attempt = 0)
        {
            try
            {
                Handler.Save(GetUniqueFilePath(fileName));
                AttemptTransform(transform, fileName);
            }
            catch (Exception ex)
            {
                if (attempt < MaxRetries)
                {
                    Console.WriteLine($"Error writing {fileName}: {ex.Message}. Retrying...");
                    Thread.Sleep(RetryDelayMs);
                    AttemptSave(transform, fileName, ++attempt);
                }
                else
                {
                    Console.WriteLine($"Failed to save {fileName} after {MaxRetries} attempts.");
                }
            }
        }

        private void AttemptTransform(bool transform, string fileName)
        {
            try
            {
                if (transform && Handler is ITransformableOutput transformable)
                {
                    transformable.TransformAndSave(GetUniqueFilePath(fileName));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error transforming {fileName} to XML: {ex}");
            }
        }
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
