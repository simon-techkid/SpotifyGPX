// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SpotifyGPX.Options;

# nullable enable

namespace SpotifyGPX.PointPredict
{
    public class PointPredict
    {
        public static List<(SpotifyEntry, GPXPoint, int)> PredictPoints(List<(SpotifyEntry, GPXPoint, int)> finalPoints, string? kmlFile)
        {
            Console.WriteLine("[PRED] Scanning for duplicate entries:");

            // Create list of grouped duplicate coordinate values from final points list
            var groupedDuplicates = finalPoints
            .GroupBy(p => (p.Item2.Latitude, p.Item2.Longitude));

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
                Console.WriteLine($"     - {string.Join(", ", group.Select(s => $"{s.Item1.Song_Name} ({s.Item3})"))}");

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
            (double, double) startPoint = (finalPoints[startIndex].Item2.Latitude, finalPoints[startIndex].Item2.Longitude);
            (double, double) endPoint = (finalPoints[endIndex].Item2.Latitude, finalPoints[endIndex].Item2.Longitude);

            // Calculate the number of dupes based on the difference between the start and end values
            int dupes = endIndex - startIndex;

            if (dupes < 2)
            {
                throw new Exception("A dupe must constitute 2 or more songs!");
            }

            // Generate a list of intermediate points based on the start, end, and number of points
            List<GPXPoint> intermediates = (kmlFile != null ? CalculateKmlIntermediates(kmlFile, dupes, startPoint, endPoint) : GenerateIntermediates(startPoint, endPoint, dupes))
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
                var (song, point, _) = finalPoints[layer];

                // Create a new GPXPoint with updated latitude and longitude (from intermediate calculation
                GPXPoint updatedPoint = new()
                {
                    Predicted = true, // Inform description this is a predicted entry
                    Time = finalPoints[layer].Item1.Time_End, // get time from song end time
                    Latitude = intermediates[index].Latitude,
                    Longitude = intermediates[index].Longitude
                };

                // Replaced indexedPoints index with the updated point
                finalPoints[layer] = (song, updatedPoint, layer);

                Console.WriteLine($"[DUPE] [{layer}] {(updatedPoint.Latitude, updatedPoint.Longitude)} {song.Song_Name}");
            }

            // Return the updated points list
            return finalPoints;
        }

        private static (double, double)[] GenerateIntermediates((double, double) start, (double, double) end, int dupes)
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

        private static (double, double)[] CalculateKmlIntermediates(string kmlFile, int dupes, (double, double) startPoint, (double, double) endPoint)
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
    }
}
