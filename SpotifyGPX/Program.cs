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
        if (args.Length == 1)
        {
            if (args[0] == "-n")
            {
                Console.WriteLine("[HELP] Pass -n to complete a song-point pairing without sending the pairs to a GPX file");
                return;
            }
            else if (args[0] == "-j")
            {
                Console.WriteLine("[HELP] Pass -j to export a JSON of the songs covering your journey");
                return;
            }
            else if (args[0] == "-p")
            {
                Console.WriteLine("[HELP] Pass -p to export a XSPF playlist of the songs covering your journey");
                return;
            }
            else if (args[0] == "-s")
            {
                Console.WriteLine("[HELP] Pass -s to export and copy to clipboard a TXT list of Spotify tracks you can paste into a playlist on Spotify");
                return;
            }
            else if (args[0] == "-g")
            {
                Console.WriteLine("[HELP] Pass -g to enable coordinate-absence song placement in the resulting GPX (cannot use -g with -n)");
                return;
            }
        }

        if (args.Length >= 2 && ".json" == Path.GetExtension(args[0]) && ".gpx" == Path.GetExtension(args[1]))
        {
            string inputJson = args[0];
            string inputGpx = args[1];
            bool noGpxExport = args.Length >= 3 && args.Contains("-n");
            bool exportJson = args.Length >= 3 && args.Contains("-j");
            bool exportPlist = args.Length >= 3 && args.Contains("-p");
            bool exportSpotifyURI = args.Length >= 3 && args.Contains("-s");
            bool predictPoints = args.Length >= 3 && args.Contains("-g");

            if (!File.Exists(inputJson))
            {
                // Ensures the specified JSON exists
                Console.WriteLine($"[INFO] Source file, '{Path.GetFileName(inputJson)}', does not exist!");
                return;
            }
            else if (!File.Exists(inputGpx))
            {
                // Ensures the specified GPX exists
                Console.WriteLine($"[INFO] Source file, '{Path.GetFileName(inputGpx)}', does not exist!");
                return;
            }

            string outputGpx = GenerateOutputPath(inputGpx, "gpx");

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
                string kmlFile = GenerateOutputPath(inputGpx, "kml");

                pairedEntries = new(pairedEntries, kmlFile);
            }

            if (noGpxExport == false)
            {
                string desc = $"Arguments: {string.Join(", ", args)}";

                // Create a GPX document based on the list of songs and points
                if (pairedEntries.CreateGPX(outputGpx, desc))
                {
                    Console.WriteLine($"[GPX] GPX file, '{Path.GetFileName(outputGpx)}', generated successfully");
                }
                else
                {
                    Console.WriteLine($"[ERROR] Error saving paired points to GPX at '{Path.GetFileName(outputGpx)}'");
                }
            }

            if (exportJson == true)
            {
                // Stage output path of output JSON
                string outputJson = GenerateOutputPath(inputGpx, "json");

                pairedEntries.JsonToFile(outputJson);

                Console.WriteLine($"[JSON] JSON file, '{Path.GetFileName(outputJson)}', generated successfully");
            }

            if (exportPlist == true)
            {
                // Stage output path of output XSPF
                string outputPlist = GenerateOutputPath(inputGpx, "xspf");

                pairedEntries.PlaylistToFile(outputPlist);

                Console.WriteLine($"[XSPF] XSPF file, {Path.GetFileName(outputPlist)}', generated successfully");
            }

            if (exportSpotifyURI == true)
            {
                // Stage output path of output URI list
                string outputTxt = GenerateOutputPath(inputGpx, "txt");

                pairedEntries.JsonUriToFile(outputTxt);

                Console.WriteLine($"[TXT] TXT file, '{Path.GetFileName(outputTxt)}', generated successfully");
            }
        }
        else
        {
            // None of these

            Console.WriteLine("[ERROR] Usage: SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-s] [-g]");
            return;
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
