using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    private static bool ReplaceFiles => true;
    private readonly IEnumerable<SongPoint> Pairs;
    private readonly Dictionary<Formats, Lazy<IFileOutput>> formatHandlers;

    public OutputHandler(IEnumerable<SongPoint> pairs)
    {
        Pairs = pairs;

        formatHandlers = new Dictionary<Formats, Lazy<IFileOutput>>
            {
                { Formats.Gpx, new Lazy<IFileOutput>(() => new Gpx(pairs)) },
                { Formats.Json, new Lazy<IFileOutput>(() => new Json(pairs)) },
                { Formats.JsonReport, new Lazy<IFileOutput>(() => new JsonReport(pairs)) },
                { Formats.Txt, new Lazy<IFileOutput>(() => new Txt(pairs)) },
                { Formats.Xspf, new Lazy<IFileOutput>(() => new Xspf(pairs)) }
            };
    }

    public void Save(string name, Formats format)
    {
        string uppercaseFormat = format.ToString().ToUpper();
        string lowercaseFormat = format.ToString().ToLower();        
        string outputFileName = GetOutputFileName(name, lowercaseFormat);
        string path = GetUniqueFilePath(outputFileName);

        IFileOutput export = formatHandlers[format].Value;
        export.Save(path);

        Console.WriteLine($"[OUT] {uppercaseFormat} [{export.Count}/{Pairs.Count()}]");
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
    int Count { get; } // Provides the number of pairings in the file
    void Save(string path); // Allows the saving of that file to the local disk
}