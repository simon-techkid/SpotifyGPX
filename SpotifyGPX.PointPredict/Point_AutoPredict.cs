// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SpotifyGPX.PointPredict;

public partial class PointPredict
{
    public static List<SongPoint> AutoPredict(List<SongPoint> finalPoints, string? kmlFile)
    {
        Console.WriteLine("[INFO] Scanning for duplicate entries:");

        // Create list of grouped duplicate coordinate values from final points list
        var groupedDuplicates = finalPoints
        .GroupBy(p => (p.Point.Latitude, p.Point.Longitude));

        foreach (var group in groupedDuplicates)
        {
            // For every set of duplicates

            // Parse each duplicated song/point to a list
            List<SongPoint> duplicateSongs = group.ToList();

            // Ensure the group constsitutes a duplicate
            if (duplicateSongs.Count < 2)
            {
                // Skip this group if it does not include two or more songs
                continue;
            }

            // Print the songs implicated and their indexes to the console
            Console.WriteLine($"       {string.Join(", ", group.Select(s => $"{s.Song.Song_Name} ({s.Song.Index})"))}");

            // Generate start and end point coordinate doubles of the specified start and end duplicates
            (double, double) startPoint = (duplicateSongs[0].Point.Latitude, duplicateSongs[0].Point.Longitude);
            (double, double) endPoint = (finalPoints[duplicateSongs[duplicateSongs.Count - 1].Song.Index + 1].Point.Latitude, finalPoints[duplicateSongs[duplicateSongs.Count - 1].Song.Index + 1].Point.Longitude);

            // Generate a list of intermediate points based on the start, end, and number of points
            List<GPXPoint> intermediates = (kmlFile != null ? GenerateKmlIntermediates(kmlFile, duplicateSongs.Count, startPoint, endPoint) : GenerateEquidistantIntermediates(startPoint, endPoint, duplicateSongs.Count))
            .Select(point => new GPXPoint
            {
                Latitude = point.Item1,
                Longitude = point.Item2
            })
            .ToList();

            // Iterate through the songs inplicated in this dupe cluster
            for (int index = 0; index < intermediates.Count; index++)
            {
                int layer = duplicateSongs[0].Song.Index + index;

                SongPoint originalPair = finalPoints[layer];

                //var (song, point, _) = finalPoints[layer];

                // Create a new GPXPoint with updated latitude and longitude
                GPXPoint updatedPoint = new()
                {
                    Predicted = true,
                    Time = finalPoints[layer].Song.Time_End,
                    Latitude = intermediates[index].Latitude,
                    Longitude = intermediates[index].Longitude
                };

                SongPoint updatedPair = new()
                {
                    Song = originalPair.Song,
                    Point = updatedPoint
                };

                // Update the indexedPoints list with the new GPXPoint
                finalPoints[layer] = updatedPair;

                Console.WriteLine($"[DUPE] [{layer}] {(updatedPoint.Latitude, updatedPoint.Longitude)} {originalPair.Song.Song_Name}");
            }
        }

        // Return the updated points list
        return finalPoints;
    }
}
