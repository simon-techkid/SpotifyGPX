// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SpotifyGPX.Pairings;

public struct Pairings
{
    public Pairings(List<SpotifyEntry> s, List<GPXPoint> p)
    {
        PairedPoints = PairPoints(s, p);
    }

    private List<SongPoint> PairedPoints;

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXPoint> points)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<SongPoint> correlatedEntries = songs
        .Select(spotifyEntry =>
        {
            GPXPoint nearestPoint = points
            .OrderBy(point => Math.Abs((point.Time - spotifyEntry.Time).TotalSeconds))
            .First();

            SongPoint pair = new(spotifyEntry, nearestPoint);
            Console.WriteLine(pair.ToString());

            return pair;
        })
        .ToList();

        if (correlatedEntries.Count > 0)
        {
            // Calculate and print the average correlation accuracy in seconds
            Console.WriteLine($"[CORR] Song-Point Correlation Accuracy (avg sec): {Math.Round(correlatedEntries.Average(correlatedPair => correlatedPair.AbsAccuracy))}");
        }

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }

    public void PredictPoints(string? kmlFile)
    {
        Console.WriteLine("[PRED] Scanning for duplicate entries:");

        // Create list of grouped duplicate coordinate values from final points list
        var groupedDuplicates = PairedPoints
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
        (double, double) startPoint = (PairedPoints[startIndex].Point.Latitude, PairedPoints[startIndex].Point.Longitude);
        (double, double) endPoint = (PairedPoints[endIndex].Point.Latitude, PairedPoints[endIndex].Point.Longitude);

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
            SongPoint originalPoint = PairedPoints[layer];

            // Create a new GPXPoint with updated latitude and longitude (from intermediate calculation
            GPXPoint updatedPoint = new()
            {
                Predicted = true, // Inform description this is a predicted entry
                Time = PairedPoints[layer].Song.Time, // get time from song end time
                Latitude = intermediates[index].Latitude,
                Longitude = intermediates[index].Longitude
            };

            SongPoint updatedPair = new(originalPoint.Song, updatedPoint);

            // Replaced PairedPoints index with the updated point
            PairedPoints[layer] = updatedPair;

            Console.WriteLine($"[DUPE] [{layer}] {(updatedPoint.Latitude, updatedPoint.Longitude)} {originalPoint.Song.Song_Name}");
        }

        // Return the updated points list
        return;
    }

    public void AutoPredict(string? kmlFile)
    {
        Console.WriteLine("[INFO] Scanning for duplicate entries:");

        // Create list of grouped duplicate coordinate values from final points list
        var groupedDuplicates = PairedPoints
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
            (double, double) endPoint = (PairedPoints[duplicateSongs[duplicateSongs.Count - 1].Song.Index + 1].Point.Latitude, PairedPoints[duplicateSongs[duplicateSongs.Count - 1].Song.Index + 1].Point.Longitude);

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

                SongPoint originalPair = PairedPoints[layer];

                // Create a new GPXPoint with updated latitude and longitude
                GPXPoint updatedPoint = new()
                {
                    Predicted = true,
                    Time = PairedPoints[layer].Song.Time,
                    Latitude = intermediates[index].Latitude,
                    Longitude = intermediates[index].Longitude
                };

                SongPoint updatedPair = new(originalPair.Song, updatedPoint);

                // Update the indexedPoints list with the new GPXPoint
                PairedPoints[layer] = updatedPair;

                Console.WriteLine($"[DUPE] [{layer}] {(updatedPoint.Latitude, updatedPoint.Longitude)} {originalPair.Song.Song_Name}");
            }
        }

        // Return the updated points list
        return;
    }

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
            double intermediateLon = startLon + average * (endLon - startLon);

            // Replace the list entry with the intermediate point
            intermediatePoints[iteration] = (intermediateLat, intermediateLon);
        }

        // Return the updated point list
        return intermediatePoints;
    }

    private static (double, double)[] GenerateKmlIntermediates(string kmlFile, int dupes, (double, double) startPoint, (double, double) endPoint)
    {
        List<(double, double)> kmlPoints = ParseKmlFile(kmlFile);

        (double, double) firstPoint = kmlPoints.OrderBy(coord => CalculateDistance(startPoint, coord)).First();
        (double, double) lastPoint = kmlPoints.OrderBy(coord => CalculateDistance(endPoint, coord)).First();

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
        // sqrt((lat2 - lat1)^2 + (lon2 - lon1)^2)

        double latDiff = coord2.Item1 - coord1.Item1; // Difference in latitudes between first and second coord
        double lonDiff = coord2.Item2 - coord1.Item2; // Difference in longitudes

        return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
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

    public readonly XmlDocument CreateGPX(string gpxFile, string desc)
    {
        // Create a new XML document
        XmlDocument document = new();

        // Create the XML header
        XmlNode header = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(header);

        // Create the GPX header
        XmlElement GPX = document.CreateElement("gpx");
        document.AppendChild(GPX);

        // Add GPX header attributes
        GPX.SetAttribute("version", "1.0");
        GPX.SetAttribute("creator", "SpotifyGPX");
        GPX.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
        GPX.SetAttribute("xmlns", "http://www.topografix.com/GPX/1/0");
        GPX.SetAttribute("xsi:schemaLocation", "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd");

        // Add name of GPX file, based on input GPX name
        XmlElement gpxName = document.CreateElement("name");
        gpxName.InnerText = Path.GetFileNameWithoutExtension(gpxFile);
        GPX.AppendChild(gpxName);

        // Add description of GPX file, based on file's creation
        XmlElement gpxDesc = document.CreateElement("desc");
        gpxDesc.InnerText = desc;
        GPX.AppendChild(gpxDesc);

        // Add description of GPX file, based on file's creation
        XmlElement gpxAuthor = document.CreateElement("author");
        gpxAuthor.InnerText = "SpotifyGPX";
        GPX.AppendChild(gpxAuthor);

        // Add time of GPX file, based on file's creation time
        XmlElement gpxTime = document.CreateElement("time");
        gpxTime.InnerText = DateTime.Now.ToUniversalTime().ToString(Point.gpxTimeOut);
        GPX.AppendChild(gpxTime);

        double pointCount = 0;

        foreach (SongPoint pair in PairedPoints)
        {
            // Create waypoint for each song
            XmlElement waypoint = document.CreateElement("wpt");
            GPX.AppendChild(waypoint);

            // Set the lat and lon of the waypoing to the original point
            waypoint.SetAttribute("lat", pair.Point.Latitude.ToString());
            waypoint.SetAttribute("lon", pair.Point.Longitude.ToString());

            // Set the name of the GPX point to the name of the song
            XmlElement name = document.CreateElement("name");
            name.InnerText = pair.GpxTitle();
            waypoint.AppendChild(name);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = pair.Point.Time.ToUniversalTime().ToString(Point.gpxTimeOut);
            waypoint.AppendChild(time);

            // Set the description of the point 
            XmlElement description = document.CreateElement("desc");
            description.InnerText = pair.GpxDescription();
            waypoint.AppendChild(description);

            pointCount++;
        }

        Console.WriteLine($"[GPX] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFile)}' added to GPX");

        return document;
    }
}