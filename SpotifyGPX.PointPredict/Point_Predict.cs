// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyGPX.Options;

#nullable enable

namespace SpotifyGPX.PointPredict;

public partial class PointPredict
{
    public static List<SongPoint> PredictPoints(List<SongPoint> finalPoints, string? kmlFile)
    {
        Console.WriteLine("[PRED] Scanning for duplicate entries:");

        // Create list of grouped duplicate coordinate values from final points list
        var groupedDuplicates = finalPoints
        .GroupBy(p => (p.Point.Latitude, p.Point.Longitude));

        foreach (var group in groupedDuplicates)
        {
            // For every set of duplicates

            // Ensure the group constsitutes a duplicate
            if (group.ToList().Count < 2)
            {
                // Skip this group if it does not include two or more songs
                continue;
            }

            // Print the songs implicated and their indexes to the console
            Console.WriteLine($"     - {string.Join(", ", group.Select(s => $"{s.Song.Song_Name} ({s.Song.Index})"))}");

        }

        // Create variables to hold the index of the beginning and end of the dupe sequence
        int startIndex = 0;
        int endIndex = 0;

        // Attempt to retrieve user input about duplicates
        try
        {
            // Ask the user where the targeted dupe starts and ends
            Console.Write("[PRED] Index of the Start of your dupe: ");
            startIndex = int.Parse(Console.ReadLine());
            Console.Write("[PRED] Index of the End of your dupe: ");
            endIndex = int.Parse(Console.ReadLine());
        }
        catch (FormatException)
        {
            throw new FormatException($"You must enter a number!");
        }

        // Generate start and end point coordinate doubles of the specified start and end duplicates
        (double, double) startPoint = (finalPoints[startIndex].Point.Latitude, finalPoints[startIndex].Point.Longitude);
        (double, double) endPoint = (finalPoints[endIndex].Point.Latitude, finalPoints[endIndex].Point.Longitude);

        // Calculate the number of dupes based on the difference between the start and end values
        int dupes = endIndex - startIndex;

        if (dupes < 2)
        {
            throw new Exception("A dupe must constitute 2 or more songs!");
        }

        // Generate a list of intermediate points based on the start, end, and number of points
        List<GPXPoint> intermediates = (kmlFile != null ? GenerateKmlIntermediates(kmlFile, dupes, startPoint, endPoint) : GenerateEquidistantIntermediates(startPoint, endPoint, dupes))
        .Select(point => new GPXPoint
        {
            Latitude = point.Item1,
            Longitude = point.Item2
        })
        .ToList();

        // Iterate through the songs inplicated in this dupe cluster
        for (int index = 0; index < dupes; index++)
        {
            // For every duped song in the cluster:

            // Calculate this dupe's index based on the start index and this iteration number
            int layer = startIndex + index;

            // Create a variable storing the original entry from finalPoints
            SongPoint originalPoint = finalPoints[layer];

            // Create a new GPXPoint with updated latitude and longitude (from intermediate calculation
            GPXPoint updatedPoint = new()
            {
                Predicted = true, // Inform description this is a predicted entry
                Time = finalPoints[layer].Song.Time_End, // get time from song end time
                Latitude = intermediates[index].Latitude,
                Longitude = intermediates[index].Longitude
            };

            SongPoint updatedPair = new()
            {
                Song = originalPoint.Song,
                Point = updatedPoint
            };

            // Replaced indexedPoints index with the updated point
            finalPoints[layer] = updatedPair;

            Console.WriteLine($"[DUPE] [{layer}] {(updatedPoint.Latitude, updatedPoint.Longitude)} {originalPoint.Song.Song_Name}");
        }

        // Return the updated points list
        return finalPoints;
    }
}
