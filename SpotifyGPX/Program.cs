// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SpotifyGPX.Options;
using SpotifyGPX.Json;
using SpotifyGPX.Gpx;
using SpotifyGPX.PointPredict;
using SpotifyGPX.Playlist;
using SpotifyGPX.Clipboard;

#nullable enable

class Program
{
    [STAThread]
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

            // Step 1: Create a list of all Spotify songs in the given JSON file
            List<SpotifyEntry> spotifyEntries;

            // Step 2: Create a list of all GPX points in the given GPX file
            List<GPXPoint> gpxPoints;

            // Step 3: Create a list of songs within the timeframe between the first and last GPX point
            List<SpotifyEntry> filteredEntries;

            // Step 4: Create a list of paired songs and points based on the closest time between each song and each GPX point
            List<(SpotifyEntry, GPXPoint, int)> correlatedEntries;

            try
            {
                // Step 1: Create list of songs contained in the JSON file, and get the JSON format
                spotifyEntries = Serializer.ParseSpotifyJson(inputJson);

                // Step 2: Create list of GPX points from the GPX file
                gpxPoints = Parser.ParseGPXFile(inputGpx);

                // Step 3: Create list of songs played during the GPX tracking timeframe
                filteredEntries = Json.FilterSpotifyJson(spotifyEntries, gpxPoints);

                // Step 4: Create list of songs and points paired as close as possible to one another
                correlatedEntries = GPX.CorrelatePoints(filteredEntries, gpxPoints);
            }
            catch (Exception ex)
            {
                // Catch any errors found in the calculation process
                Console.WriteLine(ex);
                return;
            }

            Console.WriteLine($"[INFO] {filteredEntries.Count} Spotify entries filtered from {spotifyEntries.Count} total");
            Console.WriteLine($"[INFO] {correlatedEntries.Count} Spotify entries matched to set of {filteredEntries.Count}");

            if (noGpxExport == false)
            {
                XmlDocument document;

                try
                {
                    if (predictPoints == true)
                    {
                        string kmlFile = GenerateOutputPath(inputGpx, "kml");
                        
                        correlatedEntries = PointPredict.PredictPoints(correlatedEntries, File.Exists(kmlFile) ? kmlFile : null);
                    }

                    // Create a GPX document based on the list of songs and points
                    document = Parser.CreateGPXFile(correlatedEntries, inputGpx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating GPX: {ex.Message}");
                    return;
                }

                // Write the contents of the GPX
                document.Save(outputGpx);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputGpx)} file, '{Path.GetFileName(outputGpx)}', generated successfully!");
            }

            if (exportJson == true)
            {
                // Stage output path of output JSON
                string outputJson = GenerateOutputPath(inputGpx, "json");

                try
                {
                    // Write the contents of the JSON
                    File.WriteAllText(outputJson, Serializer.ExportSpotifyJson(filteredEntries));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating JSON: {ex.Message}");
                    return;
                }

                Console.WriteLine($"[INFO] {Path.GetExtension(outputJson)} file, '{Path.GetFileName(outputJson)}', generated successfully!");
            }

            if (exportPlist == true)
            {
                // Stage output path of output XSPF
                string outputPlist = GenerateOutputPath(inputGpx, "xspf");

                XmlDocument playlist;

                try
                {
                    // Create an XML document for the playlist
                    playlist = XSPF.CreatePlist(filteredEntries, outputPlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating playlist: {ex.Message}");
                    return;
                }

                // Write the contents of the XSPF
                playlist.Save(outputPlist);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputPlist)} file, {Path.GetFileName(outputPlist)}', generated successfully!");
            }

            if (exportSpotifyURI == true)
            {
                // Stage output path of output URI list
                string outputTxt = GenerateOutputPath(inputGpx, "txt");
                string clipboard;

                // Attempt to parse SpotifyEntries for URI
                try
                {
                    // Get the list of Spotify URIs as a string
                    clipboard = Json.GenerateClipboardData(filteredEntries);
                }
                catch (Exception ex)
                {
                    // URI found to be null
                    Console.WriteLine($"Error generating clipboard data: {ex.Message}");
                    return;
                }

                // Set the clipboard contents to the string
                Clipboard.SetClipboard(clipboard);

                // Write the contents of the URI list
                File.WriteAllText(outputTxt, clipboard);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputTxt)} file, '{Path.GetFileName(outputTxt)}', generated successfully!");

                Console.WriteLine("[INFO] Spotify URIs copied to clipboard, ready to paste into a Spotify playlist!");
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
