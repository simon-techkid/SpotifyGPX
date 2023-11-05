// SpotifyGPX by Simon Field

namespace SpotifyGPX.PointPredict;

partial class PointPredict
{
    private static (double, double)[] GenerateEquidistantIntermediates((double, double) start, (double, double) end, int dupes)
    {
        // Parse start coordinate and end coordinate to lat and lon doubles
        (double startLat, double startLon) = start;
        (double endLat, double endLon) = end;

        // For each dupe, determine its equidistant point
        var intermediatePoints = new (double, double)[dupes];
        for (int iteration = 0; iteration < dupes; iteration++)
        {
            // Determine the average for this iteration based on the number of dupes between the start and end points
            double average = (double)iteration / (dupes - 1);

            // Determine the intermediate lat/lon based on the start/end point average
            double intermediateLat = startLat + average * (endLat - startLat);
            double intermediateLng = startLon + average * (endLon - startLon);

            // Replace the list entry with the intermediate point
            intermediatePoints[iteration] = (intermediateLat, intermediateLng);
        }

        // Return the updated point list
        return intermediatePoints;
    }
}
