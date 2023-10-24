// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Gpx
{
    public class GPX
    {
        public static List<(SpotifyEntry, GPXPoint, int)> CorrelatePoints(List<SpotifyEntry> filteredEntries, List<GPXPoint> gpxPoints)
        {
            // Correlate Spotify entries with the nearest GPX points
            List<(SpotifyEntry, GPXPoint, int)> correlatedEntries = new();

            // Create a list of correlation accuracies, one for each song
            List<double> correlationAccuracy = new();

            int count = 0;

            foreach (SpotifyEntry spotifyEntry in filteredEntries)
            {
                // Create variable to hold the calculated nearest GPX point and its accuracy (absolute value in comparison to each song)
                var nearestPoint = gpxPoints
                .Select(point => new
                {
                    Point = point,
                    Accuracy = Math.Abs((point.Time - spotifyEntry.Time_End).TotalSeconds)
                })
                .OrderBy(item => item.Accuracy)
                .First();

                // Add correlation accuracy (seconds) to the correlation accuracies list
                correlationAccuracy.Add(nearestPoint.Accuracy);

                // Add both the current Spotify entry and calculated nearest point to the correlated entries list
                correlatedEntries.Add((spotifyEntry, nearestPoint.Point, count));

                Console.WriteLine($"[SONG] [{count}] [{spotifyEntry.Time_End.ToUniversalTime().ToString(Point.consoleReadoutFormat)} ~ {nearestPoint.Point.Time.ToUniversalTime().ToString(Point.consoleReadoutFormat)}] [~{Math.Round(nearestPoint.Accuracy)}s] {Point.GpxTitle(spotifyEntry)}");

                count++;
            }

            // Calculate and print the average correlation accuracy in seconds
            Console.WriteLine($"[INFO] Song-Point Correlation Accuracy (avg sec): {Math.Round(Queryable.Average(correlationAccuracy.AsQueryable()))}");

            // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
            return correlatedEntries;
        }
    }
}
