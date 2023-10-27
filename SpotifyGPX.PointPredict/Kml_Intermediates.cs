// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SpotifyGPX.Options;

namespace SpotifyGPX.PointPredict;

partial class PointPredict
{
    private static (double, double)[] GenerateKmlIntermediates(string kmlFile, int dupes, (double, double) startPoint, (double, double) endPoint)
    {
        List<(double, double)> kmlPoints = ParseKmlFile(kmlFile);

        (double, double) firstPoint = kmlPoints
        .OrderBy(coord => CalculateDistance(startPoint, coord))
        .First();

        (double, double) lastPoint = kmlPoints
        .OrderBy(coord => CalculateDistance(endPoint, coord))
        .First();

        List<(double, double)> between = kmlPoints
        .OrderBy(point => CalculateDistance((firstPoint.Item1, firstPoint.Item2), point))
        .TakeWhile(point => point != lastPoint || point == firstPoint)
        .ToList();

        Console.WriteLine($"[KML] Start of dupe area: {(firstPoint.Item1, firstPoint.Item2)}");
        Console.WriteLine($"[KML] End of dupe area: {(lastPoint.Item1, lastPoint.Item2)}");

        // For each dupe, calculate its corresponding KML point
        var intermediatePoints = new (double, double)[dupes];
        for (int iteration = 0; iteration < dupes; iteration++)
        {
            // Determine the KML point to retrieve based on the number of coordinates in the KML, divided by the number of dupes, times the current iteration
            int index = between.Count / dupes * iteration;

            // Determine the intermediate lat/lon based on the KML coordinate average index
            double intermediateLat = between[index].Item1;
            double intermediateLng = between[index].Item2;

            // Replace the list entry with the intermediate point
            intermediatePoints[iteration] = (intermediateLat, intermediateLng);
        }

        // Return the updated point list
        return intermediatePoints;
    }
}
