// SpotifyGPX by Simon Field

using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpotifyGPX;

public class Pairings
{
    private static double? MaximumAbsAccuracy => null; // Greatest accepted error (in seconds) between song and point time (null = allow all pairings regardless of accuracy)

    public Pairings(List<SpotifyEntry> s, List<GPXTrack> t) => Pairs = PairPoints(s, t);
    private readonly List<SongPoint> Pairs;

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXTrack> gpxTracks)
    {
        // Correlate Spotify entries with the nearest GPX points

        int index = 0; // Index of the pairing

        return gpxTracks // For each GPX track
        .SelectMany(gpxTrack => songs // Get the list of SpotifyEntries
        .Where(spotifyEntry => spotifyEntry.WithinTimeFrame(gpxTrack.Start, gpxTrack.End)) // If the SpotifyEntry falls within the boundaries of the track
        .Select(spotifyEntry => // Select the Spotify entry if it falls in range of the GPX track
            {
                SongPoint pair = gpxTrack.Points
                .Select(point => new SongPoint(index, spotifyEntry, point, gpxTrack.Track)) // For each point in the track's point list,
                .OrderBy(pair => pair.AbsAccuracy) // Order the points by proximity between point and song
                .First(); // Closest accuracy wins

                Console.WriteLine(pair.ToString());

                index++; // Add to the index of all pairings regardless of track

                return pair;
            })
        )
        .Where(pair => MaximumAbsAccuracy == null || pair.AbsAccuracy <= MaximumAbsAccuracy) // Only create pairings with accuracy equal to or below max allowed accuracy
        .ToList();
    }

    public void Save(OutputHandler.Formats format, string name)
    {
        // Parse the Pairs in this Pairings object to the specified format, according to user-provided arguments
        OutputHandler fmat = new(Pairs);

        // Save the handled format as specified by the provided file name prefix
        fmat.Save(name, format);
    }

    public void WriteCounts()
    {
        string countsJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin).Select(track => $"{track.Count()} ({track.Key.ToString()})"));
        int trackCount = Pairs.GroupBy(pair => pair.Origin).Count(); // Total number of tracks represented
        string typesJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin.Type).Select(track => $"{track.Count()} ({track.Key.ToString()})"));
        int typeCount = Pairs.GroupBy(pair => pair.Origin.Type).Count(); // Total number of track types represented

        Console.WriteLine($"[PAIR] Paired {Pairs.Count} entries from {trackCount} tracks: {countsJoined}");
        Console.WriteLine($"[PAIR] Paired {Pairs.Count} entries of {typeCount} types: {typesJoined}");
    }

    public void WriteAverages()
    {
        // Uncomment below to group by track, rather than track type
        //string accuraciesJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin).Select(track => $"{Math.Round(track.Average(pair => pair.AbsAccuracy))}s ({track.Key.ToString()})"));

        // Calculate Accuracies for Track Types (GPX, Gap, Combined)
        string accuraciesJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin.Type).Select(track => $"{Math.Round(track.Average(pair => pair.AbsAccuracy))}s ({track.Key.ToString()})"));

        Console.WriteLine($"[PAIR] Song-Point Correlation Average Accuracies: {accuraciesJoined}");
    }
}