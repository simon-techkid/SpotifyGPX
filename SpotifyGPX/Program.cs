// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using SpotifyGPX.Observation;
using SpotifyGPX.Output;
using SpotifyGPX.Pairings;
using System;
using System.Collections.Generic;

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
        bool transform = flags.Contains("x");

        Dictionary<Formats, bool> exportOptions = GetExportOptions(flags);

        // Create a list of paired songs and points
        PairingsHandler pairedEntries;

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
        PairingsFactory factory;

        if (inputPairs != null)
            factory = new PairingsFromPairings(inputPairs, BCast, predictPoints, autoPredictPoints);
        else if (inputSpotify != null && inputGps != null)
            factory = new PairingsFromSongsAndPoints(inputSpotify, inputGps, grabApiData, predictPoints, autoPredictPoints, BCast);
        else
            throw new Exception("Neither song and GPS nor pairings files provided!");

        return factory.GetHandler();
    }
}
