// SpotifyGPX by Simon Field

using SpotifyGPX.Api;
using SpotifyGPX.Input;
using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX;

public partial class Program
{
    static void Main(string[] args)
    {
        var (options, flags) = ArgumentParser.Parse(args);

        if (args.Length == 0 || flags.Contains("h"))
        {
            ArgumentParser.PrintHelp();
            return;
        }

        string? inputPairs = options.GetValueOrDefault("pairs");
        string? inputSpotify = options.GetValueOrDefault("spotify");
        string? inputGps = options.GetValueOrDefault("gps");
        bool pointPredict = flags.Contains("pp");
        bool autoPredict = flags.Contains("pa");
        bool grabApiData = flags.Contains("a");
        bool silent = flags.Contains("s");
        bool transform = flags.Contains("x");

        Dictionary<Formats, bool> exportOptions = GetExportOptions(flags);

        // Create a list of paired songs and points
        PairingsHandler pairedEntries;

        try
        {
            pairedEntries = GetPairedEntries(inputPairs, inputSpotify, inputGps, grabApiData, silent, pointPredict, autoPredict);

            // Step 4: Write the pairing job's pair counts and averages
            pairedEntries.WriteCounts();
            pairedEntries.WriteAverages();
            pairedEntries.CheckEasterEggs();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }

        try
        {
            foreach (var option in exportOptions)
            {
                if (option.Value == true)
                {
                    OutputHandler fmat = new(pairedEntries);
                    fmat.Save(option.Key, pairedEntries.Name, transform);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }

        return;
    }

    private static PairingsHandler GetPairedEntries(string? inputPairs, string? inputSpotify, string? inputGps, bool grabApiData, bool silent, bool predictPoints, bool autoPredictPoints)
    {
        if (inputPairs != null)
            return PairFromPairs(inputPairs);
        else if (inputSpotify != null && inputGps != null)
            return PairFromSongsAndPoints(inputSpotify, inputGps, grabApiData, silent, predictPoints, autoPredictPoints);
        else
            throw new Exception("Neither song and GPS nor pairings files provided!");
    }

    private static PairingsHandler PairFromPairs(string inputPairs)
    {
        List<SongPoint> pairs = new();

        using (InputHandler input = new(inputPairs))
        {
            pairs = input.GetAllPairs();
        }

        return new PairingsHandler(pairs, Path.GetFileNameWithoutExtension(inputPairs));
    }

    private static PairingsHandler PairFromSongsAndPoints(string inputSongs, string inputGps, bool grabApiData, bool silent, bool pointPredict, bool autoPredictPoints)
    {
        // Step 0: Get input handler based on file paths
        List<GpsTrack> tracks = new();
        List<ISongEntry> songs = new();

        using (InputHandler input = new(inputSongs, inputGps))
        {
            // Step 1: Get list of GPS tracks from the GPS file
            tracks = input.GetSelectedTracks();

            // Step 2: Get list of songs played from the entries file
            songs = input.GetFilteredSongs(tracks);
            //songs = input.GetAllSongs(); // Unfiltered run
        }

        // Step 2.5: Get metadata for each song
        if (grabApiData)
            songs = new EntryMatcher(songs).MatchEntries();

        // Step 3: Create list of songs and points paired as close as possible to one another
        return new PairingsHandler(songs, tracks, inputGps, silent, pointPredict, autoPredictPoints);
    }
}
