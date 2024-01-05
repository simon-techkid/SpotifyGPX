// SpotifyGPX by Simon Field

using SpotifyGPX.Gpx;
using SpotifyGPX.Json;
using SpotifyGPX.Options;
using SpotifyGPX.Pairings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable

class Program
{
    static void Main(string[] args)
    {

        string inputJson = string.Empty; // get JSON path
        string inputGpx = string.Empty; // get GPX path
        bool noGpxExport = false;
        bool exportJson = false;
        bool exportPlist = false;
        bool exportSpotifyURI = false;
        bool predictPoints = false;

        switch (args.Length)
        {
            case 1:
                switch (args[0])
                {
                    case "-n":
                        Console.WriteLine("[HELP] Pass -n to complete a song-point pairing without sending the pairs to a GPX file");
                        break;
                    case "-j":
                        Console.WriteLine("[HELP] Pass -j to export a JSON of the songs covering your journey");
                        break;
                    case "-p":
                        Console.WriteLine("[HELP] Pass -p to export a XSPF playlist of the songs covering your journey");
                        break;
                    case "-s":
                        Console.WriteLine("[HELP] Pass -s to export and copy to clipboard a TXT list of Spotify tracks you can paste into a playlist on Spotify");
                        break;
                    case "-g":
                        Console.WriteLine("[HELP] Pass -g to enable coordinate-absence song placement in the resulting GPX (cannot use -g with -n)");
                        break;
                    default:
                        break;
                }
                break;
            case >= 2:
                inputJson = args[0];
                inputGpx = args[1];
                noGpxExport = args.Length >= 3 && args.Contains("-n");
                exportJson = args.Length >= 3 && args.Contains("-j");
                exportPlist = args.Length >= 3 && args.Contains("-p");
                exportSpotifyURI = args.Length >= 3 && args.Contains("-s");
                predictPoints = args.Length >= 3 && args.Contains("-g");
                break;
            default:
                Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-s] [-g]");
                return;
        }

        // Check the JSON file
        if (Path.GetExtension(inputJson) != ".json")
        {
            // Ensure it has a JSON extension
            Console.WriteLine($"[ERROR] Provided file, '{Path.GetFileName(inputJson)}', is not a JSON file!");
            return;
        }
        else if (!File.Exists(inputJson))
        {
            // Ensure it exists
            Console.WriteLine($"[ERROR] Provided file, '{Path.GetFileName(inputJson)}', does not exist!");
            return;
        }

        // Check the GPX file
        if (Path.GetExtension(inputGpx) != ".gpx")
        {
            // Ensure it has a GPX extension
            Console.WriteLine($"[ERROR] Provided file, '{Path.GetFileName(inputGpx)}', is not a GPX file!");
            return;
        }
        else if (!File.Exists(inputGpx))
        {
            // Ensure it exists
            Console.WriteLine($"[ERROR] Provided file, '{Path.GetFileName(inputGpx)}', does not exist!");
            return;
        }

        // Create a list of paired songs and points
        Pairings pairedEntries;

        try
        {
            // Step 1: Create list of GPX points from the GPX file
            List<GPXPoint> gpxPoints = new GpxFile(inputGpx).ParseGpxPoints();

            // Step 2: Create list of songs played, and filter it to songs played during the GPX tracking timeframe
            List<SpotifyEntry> filteredEntries = new JsonFile(inputJson).FilterSpotifyJson(gpxPoints);

            // Step 3: Create list of songs and points paired as close as possible to one another
            pairedEntries = new Pairings(filteredEntries, gpxPoints);
        }
        catch (Exception ex)
        {
            // Catch any errors found in the calculation process
            Console.WriteLine(ex);
            return;
        }

        if (predictPoints == true)
        {
            // Stage path of KML route for KML prediction
            string kmlFile = GenerateOutputPath(inputGpx, "kml");

            // Set paired entries to old paired entries, with points predicted
            pairedEntries = new(pairedEntries, File.Exists(kmlFile) ? kmlFile : null);
        }

        if (noGpxExport == false)
        {
            // Stage path of output GPX
            string outputGpx = GenerateOutputPath(inputGpx, "gpx");

            // Store running arguments
            string desc = $"Arguments: {string.Join(", ", args)}";

            // Check that the file was saved successfully
            if (pairedEntries.CreateGPX(outputGpx, desc))
            {
                Console.WriteLine($"[FILE] GPX file, '{Path.GetFileName(outputGpx)}', generated successfully");
            }
            else
            {
                Console.WriteLine($"[ERROR] Error saving GPX file to: '{Path.GetFileName(outputGpx)}'");
            }
        }

        if (exportJson == true)
        {
            // Stage path of output JSON
            string outputJson = GenerateOutputPath(inputGpx, "json");

            // Check that the file was saved successfully
            if (pairedEntries.JsonToFile(outputJson))
            {
                Console.WriteLine($"[FILE] JSON file, '{Path.GetFileName(outputJson)}', generated successfully");
            }
            else
            {
                Console.WriteLine($"[ERROR] Error saving JSON file to: '{Path.GetFileName(outputJson)}'");

            }
        }

        if (exportPlist == true)
        {
            // Stage path of output XSPF
            string outputPlist = GenerateOutputPath(inputGpx, "xspf");

            // Check that the file was saved successfully
            if (pairedEntries.PlaylistToFile(outputPlist))
            {
                Console.WriteLine($"[FILE] XSPF file, {Path.GetFileName(outputPlist)}', generated successfully");
            }
            else
            {
                Console.WriteLine($"[ERROR] Error saving XSPF file to: '{Path.GetFileName(outputPlist)}'");
            }
        }

        if (exportSpotifyURI == true)
        {
            // Stage path of output URI list
            string outputTxt = GenerateOutputPath(inputGpx, "txt");

            // Check that the file was saved successfully
            if (pairedEntries.JsonUriToFile(outputTxt))
            {
                Console.WriteLine($"[FILE] TXT file, '{Path.GetFileName(outputTxt)}', generated successfully");
            }
            else
            {
                Console.WriteLine($"[ERROR] Error saving TXT file to: '{Path.GetFileName(outputTxt)}'");
            }
        }

        // Exit the program
        return;
    }

    private static string GenerateOutputPath(string inputFile, string format)
    {
        // Set up the output file path
        string outputFile = Path.Combine(Directory.GetParent(inputFile).ToString(), $"{Path.GetFileNameWithoutExtension(inputFile)}_Spotify.{format}");

        return outputFile;
    }
}
