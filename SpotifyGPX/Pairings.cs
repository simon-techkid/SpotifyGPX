// SpotifyGPX by Simon Field

using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpotifyGPX;

public class Pairings
{
    private readonly List<SongPoint> Pairs;

    public Pairings(List<SpotifyEntry> s, List<GPXTrack> t) => Pairs = PairPoints(s, t);

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXTrack> gpxTracks)
    {
        // Correlate Spotify entries with the nearest GPX points

        int index = 0; // Index of the pairing

        List<SongPoint> correlatedEntries = gpxTracks // For each GPX track
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
        .Where(pair => Options.MaximumAbsAccuracy == null || pair.AbsAccuracy <= Options.MaximumAbsAccuracy) // Only create pairings with accuracy equal to or below max allowed accuracy
        .ToList();

        if (correlatedEntries.Count > 0)
        {
            // Calculate and print the average correlation accuracy in seconds
            Console.WriteLine($"[PAIR] Song-Point Correlation Accuracy (avg sec): {Math.Round(correlatedEntries.Average(correlatedPair => correlatedPair.AbsAccuracy))}");
        }

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }

    public void Save(Formats format, string prefix, string directory)
    {
        FormatHandler fmat = new(Pairs, format);

        fmat.Save(prefix);
    }

    public override string ToString()
    {
        string countsJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin).Select(track => $"{track.Count()} ({track.Key.ToString()})"));

        return $"[PAIR] Paired {Pairs.Count} entries from {Pairs.GroupBy(pair => pair.Origin).Count()} tracks: {countsJoined}";
    }
}