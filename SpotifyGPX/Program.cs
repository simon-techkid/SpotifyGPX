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
        if (args.Length < 2)
        {
            Console.WriteLine("[HELP] Usage: SpotifyGPX <spotify> <gps> [-n] [-j] [-p] [-t] [-r] [-pp] [-pa] [--silent]");
            return;
        }

        string inputSpotify = args[0];
        string inputGps = args[1];

        bool noGpxExport = args.Contains("-n");
        bool exportJson = args.Contains("-j");
        bool exportPlist = args.Contains("-p");
        bool exportTxt = args.Contains("-t");
        bool exportJsonReport = args.Contains("-r");
        bool pointPredict = args.Contains("-pp");
        bool autoPredict = args.Contains("-pa");
        bool silent = args.Contains("--silent");

        // Create a list of paired songs and points
        PairingsHandler pairedEntries;

        try
        {
            // Step 0: Get input handler based on file paths
            InputHandler input = new(inputSpotify, inputGps);

            // Step 1: Get list of GPX tracks from the GPS file
            List<GPXTrack> gpsTracks = input.GetAllTracks();

            // Step 2: Get list of songs played from the entries file
            List<SpotifyEntry> filSongs = input.GetFilteredSongs(gpsTracks);
            //List<SpotifyEntry> allSongs = input.GetAllSongs(); // Unfiltered run

            // Step 3: Create list of songs and points paired as close as possible to one another
            pairedEntries = new PairingsHandler(filSongs, gpsTracks, silent, pointPredict, autoPredict);

            // Step 4: Write the pairing job's pair counts and averages
            pairedEntries.WriteCounts();
            pairedEntries.WriteAverages();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }

        if (!noGpxExport)
            pairedEntries.Save(Formats.Gpx, Path.GetFileNameWithoutExtension(inputGps));

        if (exportJson)
            pairedEntries.Save(Formats.Json, Path.GetFileNameWithoutExtension(inputGps));

        if (exportPlist)
            pairedEntries.Save(Formats.Xspf, Path.GetFileNameWithoutExtension(inputGps));

        if (exportTxt)
            pairedEntries.Save(Formats.Txt, Path.GetFileNameWithoutExtension(inputGps));

        if (exportJsonReport)
            pairedEntries.Save(Formats.JsonReport, Path.GetFileNameWithoutExtension(inputGps));

        return;
    }
}
