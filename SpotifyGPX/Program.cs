// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX;

class Program
{
    static void Main(string[] args)
    {
        string inputJson = string.Empty; // get JSON path
        string inputGpx = string.Empty; // get GPX path
        bool noGpxExport = false;
        bool exportJson = false;
        bool exportPlist = false;
        bool exportTxt = false;
        bool exportJsonReport = false;

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
                    case "-t":
                        Console.WriteLine("[HELP] Pass -t to export a TXT list of pairs");
                        break;
                    case "-r":
                        Console.WriteLine("[HELP] Press -r to export a verbose JSON report of all created pairings");
                        break;
                    default:
                        Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-s] [-r]");
                        break;
                }
                return;
            case >= 2:
                inputJson = args[0];
                inputGpx = args[1];
                noGpxExport = args.Length >= 3 && args.Contains("-n");
                exportJson = args.Length >= 3 && args.Contains("-j");
                exportPlist = args.Length >= 3 && args.Contains("-p");
                exportTxt = args.Length >= 3 && args.Contains("-t");
                exportJsonReport = args.Length >= 3 && args.Contains("-r");
                break;
            default:
                Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-t] [-r]");
                return;
        }

        // Create a list of paired songs and points
        PairingsHandler pairedEntries;

        try
        {
            // Step 0: Get input handler based on file paths
            InputHandler input = new(inputJson, inputGpx);

            // Step 1: Get list of GPX tracks from the GPS file
            List<GPXTrack> gpsTracks = input.GetAllTracks();

            // Step 2: Get list of songs played from the entries file
            //List<SpotifyEntry> allSongs = input.GetAllSongs(); // Unfiltered run
            List<SpotifyEntry> filSongs = input.GetFilteredSongs(gpsTracks); // Filtered run

            // Step 3: Create list of songs and points paired as close as possible to one another
            pairedEntries = new PairingsHandler(filSongs, gpsTracks);

            pairedEntries.WriteCounts();
            pairedEntries.WriteAverages();
        }
        catch (Exception ex)
        {
            // Catch any errors found in the calculation process
            Console.WriteLine(ex);
            return;
        }

        if (noGpxExport == false)
        {
            pairedEntries.Save(Formats.Gpx, Path.GetFileNameWithoutExtension(inputGpx));
        }

        if (exportJson == true)
        {
            pairedEntries.Save(Formats.Json, Path.GetFileNameWithoutExtension(inputGpx));
        }

        if (exportPlist == true)
        {
            pairedEntries.Save(Formats.Xspf, Path.GetFileNameWithoutExtension(inputGpx));
        }

        if (exportTxt == true)
        {
            pairedEntries.Save(Formats.Txt, Path.GetFileNameWithoutExtension(inputGpx));
        }

        if (exportJsonReport == true)
        {
            pairedEntries.Save(Formats.JsonReport, Path.GetFileNameWithoutExtension(inputGpx));
        }

        return; // Exit the program
    }
}
