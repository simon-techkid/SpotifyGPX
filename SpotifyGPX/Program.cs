// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX.Gpx;
using SpotifyGPX.Json;
using SpotifyGPX.Options;
using SpotifyGPX.Pairings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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
                        Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-s] [-g]");
                        break;
                }
                return;
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
            Console.WriteLine($"[ERROR] Provided file, '{inputJson}', is not a JSON file!");
            return;
        }
        else if (!File.Exists(inputJson))
        {
            // Ensure it exists
            Console.WriteLine($"[ERROR] Provided file, '{inputJson}', does not exist!");
            return;
        }

        // Check the GPX file
        if (Path.GetExtension(inputGpx) != ".gpx")
        {
            // Ensure it has a GPX extension
            Console.WriteLine($"[ERROR] Provided file, '{inputGpx}', is not a GPX file!");
            return;
        }
        else if (!File.Exists(inputGpx))
        {
            // Ensure it exists
            Console.WriteLine($"[ERROR] Provided file, '{inputGpx}', does not exist!");
            return;
        }

        // Create a list of paired songs and points
        Pairings pairedEntries;

        try
        {
            // Step 1: Create list of GPX points from the GPX file
            List<GPXTrack> tracks = new GpxFile(inputGpx).ParseGpxTracks();

            // Step 2: Create list of songs played, and filter it to songs played during the GPX tracking timeframe
            List<SpotifyEntry> filteredEntries = new JsonFile(inputJson).FilterSpotifyJson(tracks);

            // Step 3: Create list of songs and points paired as close as possible to one another
            pairedEntries = new Pairings(filteredEntries, tracks.SelectMany(track => track.Points).ToList());

            pairedEntries.PrintTracks();
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
            //pairedEntries = new(pairedEntries, File.Exists(kmlFile) ? kmlFile : null);
        }

        if (noGpxExport == false)
        {
            // Stage path of output GPX
            string outputGpx = GenerateOutputPath(inputGpx, "gpx");

            // Store creation options
            string desc = $"Arguments: {string.Join(", ", args)}";
            string name = Path.GetFileNameWithoutExtension(outputGpx);

            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            XDocument document = pairedEntries.GetGpx(name, desc, ns);

            // Check that there are points
            //if (document.Descendants(ns + "trkpt").Any())
            if (document.Descendants(ns + "wpt").Any())
            {
                document.Save(outputGpx);
                //Console.WriteLine($"[FILE] {document.Descendants(ns + "trkpt").Count()} song/point pairs added to '{Path.GetFileName(outputGpx)}'");
                Console.WriteLine($"[FILE] {document.Descendants(ns + "wpt").Count()} song/point pairs added to '{Path.GetFileName(outputGpx)}'");
            }
        }

        if (exportJson == true)
        {
            // Stage path of output JSON
            string outputJson = GenerateOutputPath(inputGpx, "json");

            List<JObject> entries = pairedEntries.GetJson();

            if (entries.Count > 0)
            {
                string document = JsonConvert.SerializeObject(entries, Formatting.Indented);
                File.WriteAllText(outputJson, document);
                Console.WriteLine($"[FILE] {entries.Count} song entries added to '{Path.GetFileName(outputJson)}'");
            }
        }

        if (exportPlist == true)
        {
            // Stage path of output XSPF
            string outputPlist = GenerateOutputPath(inputGpx, "xspf");

            // Store creation options
            string name = Path.GetFileNameWithoutExtension(outputPlist);

            XNamespace ns = "http://xspf.org/ns/0/";
            XDocument document = pairedEntries.GetPlaylist(name, ns);

            // Check that there are tracks
            if (document.Descendants(ns + "track").Any())
            {
                document.Save(outputPlist);
                Console.WriteLine($"[FILE] {document.Descendants(ns + "track").Count()} song/point pairs added to '{Path.GetFileName(outputPlist)}'");
            }
        }

        if (exportSpotifyURI == true)
        {
            // Stage path of output URI list
            string outputTxt = GenerateOutputPath(inputGpx, "txt");

            string?[] document = pairedEntries.GetUriList();

            // Check that there are URIs
            if (document.Length > 0 && !document.Any(s => s == null))
            {
                File.WriteAllLines(outputTxt, document);
                Console.WriteLine($"[FILE] {document.Length} song URIs added to '{Path.GetFileName(outputTxt)}'");
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
