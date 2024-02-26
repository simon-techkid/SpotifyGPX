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
        if (args.Length == 0 || args.Contains("--help"))
        {
            Console.WriteLine("[HELP] Usage: SpotifyGPX [--spotify <spotify> --gps <gps>] [--pairs <pairs>] [-n] [-c] [-j] [-p] [-t] [-r] [-pp] [-pa] [--silent] [--help]");
            return;
        }

        string? inputPairs = null;
        string? inputSpotify = null;
        string? inputGps = null;

        if (args.Length >= 2)
        {
            switch (args[0])
            {
                case "--pairs":
                    inputPairs = args[1];
                    break;
                case "--spotify":
                    inputSpotify = args[1];
                    if (args.Length >= 4 && args[2] == "--gps")
                    {
                        inputGps = args[3];
                    }
                    break;
                case "--gps":
                    inputGps = args[1];
                    if (args.Length >= 4 && args[2] == "--spotify")
                    {
                        inputSpotify = args[3];
                    }
                    break;
                default:
                    break;
            }
        }

        bool noGpxExport = args.Contains("-n");
        bool exportCsv = args.Contains("-c");
        bool exportJson = args.Contains("-j");
        bool exportPlist = args.Contains("-p");
        bool exportTxt = args.Contains("-t");
        bool exportJsonReport = args.Contains("-r");
        bool pointPredict = args.Contains("-pp");
        bool autoPredict = args.Contains("-pa");
        bool silent = args.Contains("--silent");

        // Create a list of paired songs and points
        PairingsHandler pairedEntries;

        string prefix = "";

        try
        {
            InputHandler input;

            if (inputPairs != null)
            {
                // Step 0: Get input handler based on file path
                input = new(inputPairs);
                prefix = inputPairs;

                // Step 1: Get list of pairs from the pairs file
                List<SongPoint> pairs = input.GetAllPairs();

                // Step 2: Create list of songs and points paired based on the given pairs file
                pairedEntries = new PairingsHandler(pairs);
            }
            else if (inputSpotify != null && inputGps != null)
            {
                // Step 0: Get input handler based on file paths
                input = new(inputSpotify, inputGps);
                prefix = inputGps;

                // Step 1: Get list of GPX tracks from the GPS file
                List<GPXTrack> tracks = input.GetSelectedTracks();

                // Step 2: Get list of songs played from the entries file
                List<SpotifyEntry> songs = input.GetFilteredSongs(tracks);
                //List<SpotifyEntry> songs = input.GetAllSongs(); // Unfiltered run

                // Step 3: Create list of songs and points paired as close as possible to one another
                pairedEntries = new PairingsHandler(songs, tracks, silent, pointPredict, autoPredict);
            }
            else
            {
                throw new Exception($"Neither song and GPS nor pairings files provided!");
            }

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
            pairedEntries.Save(Formats.Gpx, Path.GetFileNameWithoutExtension(prefix));

        if (exportCsv)
            pairedEntries.Save(Formats.Csv, Path.GetFileNameWithoutExtension(prefix));

        if (exportJson)
            pairedEntries.Save(Formats.Json, Path.GetFileNameWithoutExtension(prefix));

        if (exportPlist)
            pairedEntries.Save(Formats.Xspf, Path.GetFileNameWithoutExtension(prefix));

        if (exportTxt)
            pairedEntries.Save(Formats.Txt, Path.GetFileNameWithoutExtension(prefix));

        if (exportJsonReport)
            pairedEntries.Save(Formats.JsonReport, Path.GetFileNameWithoutExtension(prefix));

        return;
    }
}
