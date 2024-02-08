using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class OutputHandler
{
    private static bool ReplaceFiles => true; // Defines whether the program will be allowed to replace existing files. If false, it will create unique names
    private string GetOutputFileName(string name) => $"{name}.{TargetFormat.ToString().ToLower()}"; // Stage an output name for a file based on the name and target format

    public OutputHandler(IEnumerable<SongPoint> pairs, Formats format)
    {
        // Creates an OutputHandler class using:
        Pairs = pairs; // Pairs list
        TargetFormat = format; // User-specified format (based on arguments)
    }

    private IEnumerable<SongPoint> Pairs { get; } // Song-Point pairs to be included in resulting file

    private Formats TargetFormat { get; }

    private static readonly Dictionary<Formats, Type> FormatTypeMapping = new()
    {
        // Corresponding export format classes with their Format enum values 
        { Formats.Gpx, typeof(Gpx) },
        { Formats.Json, typeof(Json) },
        { Formats.JsonReport, typeof(JsonReport) },
        { Formats.Txt, typeof(Txt) },
        { Formats.Xspf, typeof(Xspf) }
    };

    private bool SupportsMultiTrack => TargetFormat switch
    {
        // Corresponding multi-track support specification in each export format class with their Format enum values
        Formats.Gpx => Gpx.SupportsMultiTrack,
        Formats.Json => Json.SupportsMultiTrack,
        Formats.JsonReport => JsonReport.SupportsMultiTrack,
        Formats.Txt => Txt.SupportsMultiTrack,
        Formats.Xspf => Xspf.SupportsMultiTrack,
        _ => throw new Exception("Invalid Format specified when saving pairings!")
    };

    public void Save(string name)
    {
        if (SupportsMultiTrack)
        {
            // If the specified format supports saving multiple tracks to a single file
            string fileName = GetUniqueFileName(GetOutputFileName(name)); // Get the file name for that single file
            double count = SaveFile(Pairs, fileName); // Pass the ungrouped Pairs list to SaveFile
            Console.WriteLine($"[OUT] {TargetFormat} [{count}/{Pairs.Count()}]");
        }
        else
        {
            List<(double count, TrackInfo track)> trackReturns = Pairs // Return the pair count and track info for all pairs...
            .GroupBy(pair => pair.Origin) // ...Grouped by track
            .Select(track => // For each track represented in Pairs:
            {
                TrackInfo currentTrack = track.Key; // Get the group Key (the variable the groups were created from), which is the current track
                string fileName = GetUniqueFileName(GetOutputFileName($"{name}_{currentTrack.ToString()}")); // Generate file name based on track name
                double count = SaveFile(track, fileName); // Return the saved file's pair count
                return (count, currentTrack); // Return the pair count of the file, and the track represented in the file (each file has one track each)
            })
            .ToList(); // Send the counts and track infos to a list

            string joinedValues = string.Join(", ", trackReturns.Select(tuple => $"{tuple.count} ({tuple.track.ToString()})")); // Join all tracks' counts and names
            double totalCount = trackReturns.Select(track => track.count).Sum(); // Get the sum of all pairs among the files created
            Console.WriteLine($"[OUT] {TargetFormat} [{totalCount}/{Pairs.Count()}]: {joinedValues}"); // Print result
        }
    }

    private double SaveFile(IEnumerable<SongPoint> pairs, string path)
    {
        IFileOutput export = GetFileSaver(pairs); // Get IFileOutput for the specified format, provided pairs
        export.Save(path); // Save the file to the disk at this path

        return export.Count; // Return the count of pairs in the exported file
    }

    private IFileOutput GetFileSaver(IEnumerable<SongPoint> pairs)
    {
        if (FormatTypeMapping.TryGetValue(TargetFormat, out Type fileType))
        {
            // If the target format can be matched with an export format class,
            // Return an interface that provides control over the specified file format handling
            return Activator.CreateInstance(fileType, pairs) as IFileOutput;
        }
        else
        {
            throw new Exception("Invalid Format specified when saving pairings!");
        }
    }

    private static string GetUniqueFileName(string path)
    {
        if (!File.Exists(path) || ReplaceFiles)
        {
            // If the target file doesn't already exist, or allowed to replace files, return the provided path
            return path;
        }

        // Otherwise, create a unique filename

        string directory = Path.GetDirectoryName(path); // Get directory
        string fileExtension = Path.GetExtension(path); // Get extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path); // Get filename without extension

        int count = 1;
        string uniqueFileName = path;

        do // Until an file path that doesn't have an existing file, generate a unique name
        {
            uniqueFileName = Path.Combine(directory, $"{fileNameWithoutExtension}_{count}{fileExtension}");
            count++; // Increase distinguishing number
        } while (File.Exists(uniqueFileName));

        // Return proven unexisting file path
        return uniqueFileName;
    }

    public enum Formats
    {
        Gpx, // GPX file containing pairs as waypoints
        Json, // JSON file containing the pairings' songs' original JSON
        JsonReport, // JSON file containing all pairing data
        Txt, // TXT list of all song URIs from pairings
        Xspf // XSPF playlist of all songs from pairings
    }
}

public interface IFileOutput
{
    // Defines the requirements of export format classes:
    int Count { get; } // Provides the number of pairings in the file
    void Save(string path); // Allows the saving of that file to the local disk
}