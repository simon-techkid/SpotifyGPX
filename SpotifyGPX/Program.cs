// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX;

class Program
{
    static void Main(string[] args)
    {
        string inputJson = string.Empty; // get JSON path
        string inputGpx = string.Empty; // get GPX path
        bool noGpxExport = false;
        bool exportWaypoints = false;
        bool exportJson = false;
        bool exportPlist = false;
        bool exportSpotifyURI = false;

        switch (args.Length)
        {
            case 1:
                switch (args[0])
                {
                    case "-n":
                        Console.WriteLine("[HELP] Pass -n to complete a song-point pairing without sending the pairs to a GPX file");
                        break;
                    case "-w":
                        Console.WriteLine("[HELP] Pass -w to write a GPX of waypoints for each source track");
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
                    default:
                        Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-s] [-g]");
                        break;
                }
                return;
            case >= 2:
                inputJson = args[0];
                inputGpx = args[1];
                noGpxExport = args.Length >= 3 && args.Contains("-n");
                exportWaypoints = args.Length >= 3 && args.Contains("-w");
                exportJson = args.Length >= 3 && args.Contains("-j");
                exportPlist = args.Length >= 3 && args.Contains("-p");
                exportSpotifyURI = args.Length >= 3 && args.Contains("-s");
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
            List<SpotifyEntry> filteredEntries = new JsonFile(inputJson).FilterSpotifyJson(tracks); // filtering not strictly necessary, replace with below commented line for unfiltered run
            //List<SpotifyEntry> filteredEntries = new JsonFile(inputJson).AllSongs;

            // Step 3: Create list of songs and points paired as close as possible to one another
            pairedEntries = new Pairings(filteredEntries, tracks);

            pairedEntries.PrintTracks();
        }
        catch (Exception ex)
        {
            // Catch any errors found in the calculation process
            Console.WriteLine(ex);
            return;
        }

        if (noGpxExport == false)
        {
            // Stage path of output GPX
            string outputGpx = GenerateOutputPath(inputGpx, "gpx");
            string name = Path.GetFileNameWithoutExtension(outputGpx);
            XDocument document = pairedEntries.GetGpx(name);

            // Check that there are points
            if (document.Descendants(Options.OutputNs + "trkpt").Any())
            {
                document.Save(outputGpx);
                Console.WriteLine($"[FILE] {document.Descendants(Options.OutputNs + "trkpt").Count()} song/point pairs added to '{Path.GetFileName(outputGpx)}'");
            }
        }

        if (exportWaypoints == true)
        {
            List<(TrackInfo, XDocument)> tracks = pairedEntries.GetGpxTracks();

            foreach ((TrackInfo track, XDocument document) in tracks)
            {
                string outputGpx = GenerateOutputPath(track.ToString(), "gpx");

                // Check that there are tracks
                if (document.Descendants(Options.OutputNs + "wpt").Any())
                {
                    document.Save(outputGpx);
                    Console.WriteLine($"[FILE] {document.Descendants(Options.OutputNs + "wpt").Count()} song/point pairs added to '{Path.GetFileName(outputGpx)}'");
                }
            }
        }

        if (exportJson == true)
        {
            List<(TrackInfo, List<JObject>)> tracks = pairedEntries.GetJson();

            foreach ((TrackInfo track, List<JObject> Json) in tracks)
            {
                string outputJson = GenerateOutputPath(track.ToString(), "json");

                if (Json.Count > 0)
                {
                    string document = JsonConvert.SerializeObject(Json, Formatting.Indented);
                    File.WriteAllText(outputJson, document);
                    Console.WriteLine($"[FILE] {Json.Count} song entries added to '{Path.GetFileName(outputJson)}'");
                }
            }
        }

        if (exportPlist == true)
        {
            List<(TrackInfo, XDocument)> tracks = pairedEntries.GetPlaylist();

            foreach ((TrackInfo track, XDocument document) in tracks)
            {
                string outputPlist = GenerateOutputPath(track.ToString(), "xspf");

                // Check that there are tracks
                if (document.Descendants(Options.Xspf + "track").Any())
                {
                    document.Save(outputPlist);
                    Console.WriteLine($"[FILE] {document.Descendants(Options.Xspf + "track").Count()} song/point pairs added to '{Path.GetFileName(outputPlist)}'");
                }
            }
        }

        if (exportSpotifyURI == true)
        {
            List<(TrackInfo, string?[])> tracks = pairedEntries.GetUriList();

            foreach ((TrackInfo track, string?[] document) in tracks)
            {
                string outputTxt = GenerateOutputPath(track.ToString(), "txt");

                if (!document.Any(s => s == null) && document.Length > 0)
                {
                    File.WriteAllLines(outputTxt, document);
                    Console.WriteLine($"[FILE] {document.Length} song URIs added to '{Path.GetFileName(outputTxt)}'");
                }
            }
        }

        return; // Exit the program
    }

    private static string GenerateOutputPath(string inputFile, string format) => Path.Combine(Directory.GetParent(inputFile).ToString(), $"{Path.GetFileNameWithoutExtension(inputFile)}_Spotify.{format}");
}
