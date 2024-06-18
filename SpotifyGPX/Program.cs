// SpotifyGPX by Simon Field

using SpotifyGPX.Api;
using SpotifyGPX.Broadcasting;
using SpotifyGPX.Input;
using SpotifyGPX.Observation;
using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX;

public partial class Program
{
    static void Main(string[] args)
    {
        const string BroadcasterPrefix = "MAIN";

        StringBroadcaster BCast = new()
        {
            Type = BroadcasterPrefix
        };

        ConsoleObserver Console = new(LogLevel.Pair);
        IDisposable ConsoleUnsubscriber = BCast.Subscribe(Console);

        FileObserver File = new("log.txt", System.Text.Encoding.UTF8, LogLevel.Debug);
        IDisposable FileUnsubscriber = BCast.Subscribe(File);

        var (options, flags) = ArgumentParser.Parse(args);

        if (args.Length == 0 || flags.Contains("h"))
        {
            ArgumentParser.PrintHelp(BCast);
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
        PairingsHandler pairedEntries = new(string.Empty, BCast);

        try
        {
            pairedEntries = GetPairedEntries(inputPairs, inputSpotify, inputGps, ref BCast, grabApiData, pointPredict, autoPredict);
            pairedEntries.WriteCounts();
            pairedEntries.WriteAverages();
            pairedEntries.CheckEasterEggs();
        }
        catch (Exception ex)
        {
            BCast.BroadcastError(ex);
            return;
        }

        try
        {
            foreach (var option in exportOptions)
            {
                if (option.Value == true)
                {
                    OutputHandler fmat = new(pairedEntries, BCast);
                    fmat.Save(option.Key, pairedEntries.Name, transform);
                }
            }
        }
        catch (Exception ex)
        {
            BCast.BroadcastError(ex);
            return;
        }

        ConsoleUnsubscriber.Dispose();
        FileUnsubscriber.Dispose();

        return;
    }

    private static PairingsHandler GetPairedEntries(string? inputPairs, string? inputSpotify, string? inputGps, ref StringBroadcaster BCast, bool grabApiData, bool predictPoints, bool autoPredictPoints)
    {
        if (inputPairs != null)
            return PairFromPairs(inputPairs, ref BCast);
        else if (inputSpotify != null && inputGps != null)
            return PairFromSongsAndPoints(inputSpotify, inputGps, ref BCast, grabApiData, predictPoints, autoPredictPoints);
        else
            throw new Exception("Neither song and GPS nor pairings files provided!");
    }

    private static PairingsHandler PairFromPairs(string inputPairs, ref StringBroadcaster BCast)
    {
        List<SongPoint> pairs = new();

        using (InputHandler input = new(inputPairs, BCast))
        {
            pairs = input.GetAllPairs();
        }

        PairingsHandler pairer = new(pairs, Path.GetFileNameWithoutExtension(inputPairs), BCast);
        return pairer;
    }

    private static PairingsHandler PairFromSongsAndPoints(string inputSongs, string inputGps, ref StringBroadcaster BCast, bool grabApiData, bool pointPredict, bool autoPredictPoints)
    {
        // Step 0: Get input handler based on file paths
        List<GpsTrack> tracks = new();
        List<ISongEntry> songs = new();

        using (InputHandler input = new(inputSongs, inputGps, BCast))
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
        PairingsHandler pairer = new(Path.GetFileNameWithoutExtension(inputGps), BCast);
        pairer.CalculatePairings(songs, tracks, pointPredict, autoPredictPoints);
        return pairer;
    }
}
