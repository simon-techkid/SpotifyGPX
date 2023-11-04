// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Correlator;

public partial class Correlate
{
    public static List<SongPoint> CorrelatePoints(List<SpotifyEntry> filteredEntries, List<GPXPoint> gpxPoints)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<SongPoint> correlatedEntries = filteredEntries
        .Select(spotifyEntry =>
        {
            var nearestPoint = gpxPoints
            .Select(point => new
            {
                Point = point,
                Accuracy = Math.Abs((point.Time - spotifyEntry.Time_End).TotalSeconds)
            })
            .OrderBy(item => item.Accuracy)
            .First();

            Console.WriteLine($"[CORR] [{nearestPoint.Point.TrackMember}] [{spotifyEntry.Index}] [{spotifyEntry.Time_End.ToUniversalTime().ToString(Point.consoleReadoutFormat)} ~ {nearestPoint.Point.Time.ToUniversalTime().ToString(Point.consoleReadoutFormat)}] [~{Math.Round(nearestPoint.Accuracy)}s] {Point.GpxTitle(spotifyEntry)}");

            return new SongPoint
            {
                Accuracy = (spotifyEntry.Time_End - nearestPoint.Point.Time).TotalSeconds,
                AbsAccuracy = nearestPoint.Accuracy,
                Song = spotifyEntry,
                Point = nearestPoint.Point
            };
        })
        .ToList();

        // Calculate and print the average correlation accuracy in seconds
        Console.WriteLine($"[CORR] Song-Point Correlation Accuracy (avg sec): {Math.Round(correlatedEntries.Average(correlatedPair => correlatedPair.AbsAccuracy))}");

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }
}
