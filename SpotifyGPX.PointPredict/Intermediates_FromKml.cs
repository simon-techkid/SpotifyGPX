// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

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

    private static double CalculateDistance((double, double) coord1, (double, double) coord2)
    {
        double lat1 = coord1.Item1;
        double lon1 = coord1.Item2;
        double lat2 = coord2.Item1;
        double lon2 = coord2.Item2;

        double latDiff = lat2 - lat1;
        double lonDiff = lon2 - lon1;

        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        return distance;
    }

    private static List<(double, double)> ParseKmlFile(string kmlFile)
    {
        // Create a new XML document
        XmlDocument doc = new();

        // Use the GPX namespace
        XmlNamespaceManager nsManager = new(doc.NameTable);
        nsManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

        // Create a list of intermediate coordinates
        List<(double, double)> coordinates = new();

        try
        {
            // Attempt to load the contents of the specified file into the XML
            doc.Load(kmlFile);
        }
        catch (Exception ex)
        {
            // If the specified XML is invalid, throw an error
            throw new Exception($"The defined {Path.GetExtension(kmlFile)} file is incorrectly formatted: {ex.Message}");
        }

        try
        {
            // Select all LineString coordinates
            XmlNodeList coordinatesNodes = doc.SelectNodes("//kml:LineString/kml:coordinates", nsManager);

            foreach (XmlNode coordinatesNode in coordinatesNodes)
            {
                string[] coordinateStrings = coordinatesNode.InnerText.Trim().Split(' ');

                foreach (string coordinateString in coordinateStrings)
                {
                    string[] parts = coordinateString.Split(',');

                    if (parts.Length >= 2 && double.TryParse(parts[0], out double longitude) && double.TryParse(parts[1], out double latitude))
                    {
                        coordinates.Add((latitude, longitude));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing {Path.GetExtension(kmlFile)} file: {ex.Message}");
        }

        List<GPXPoint> kmlPoints = coordinates
        .Select(point => new GPXPoint
        {
            Latitude = point.Item1,
            Longitude = point.Item2
        })
        .ToList();

        return coordinates;
    }
}
