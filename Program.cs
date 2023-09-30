// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpotifyGPX;
using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        if (".json" == Path.GetExtension(args[0]) && ".gpx" == Path.GetExtension(args[1]))
        {
            string inputJson = args[0];
            string inputGpx = args[1];
            bool exportJson = args.Length == 3 && args[2] == "-j";

            if (!File.Exists(inputJson))
            {
                // Ensures the specified JSON exists
                Console.WriteLine($"[INFO] Source {Path.GetExtension(inputJson)} file, '{Path.GetFileName(inputJson)}', does not exist!");
                return;
            }
            else if (!File.Exists(inputGpx))
            {
                // Ensures the specified GPX exists
                Console.WriteLine($"[INFO] Source {Path.GetExtension(inputGpx)} file, '{Path.GetFileName(inputGpx)}', does not exist!");
                return;
            }

            // Stage an output path for the resulting GPX file
            string outputGpx = Spotify.GenerateOutputPath(inputGpx, "gpx");

            // Create a list of all Spotify songs in the given JSON file
            List<SpotifyEntry> spotifyEntries = Options.ParseSpotifyJson(inputJson);

            // Create a list of all GPX points in the given GPX file
            List<GPXPoint> gpxPoints = GPX.ParseGPXFile(inputGpx);

            // Create a list of songs within the timeframe between the first and last GPX point
            List<SpotifyEntry> filteredEntries = Spotify.FilterSpotifyJson(spotifyEntries, gpxPoints);

            // Create a list of paired songs and points based on the closest time between each song and each GPX point
            List<(SpotifyEntry, GPXPoint)> correlatedEntries = Spotify.CorrelateGpxPoints(filteredEntries, gpxPoints);

            // Create a GPX document based on the list of songs and points
            XmlDocument document = GPX.CreateGPXFile(correlatedEntries, Path.GetFileName(inputGpx));

            // Save the GPX to the file
            document.Save(outputGpx);

            if (exportJson == true)
            {
                // Stage an output path for the resulting JSON file
                string outputJson = Spotify.GenerateOutputPath(inputGpx, "json");

                // Write the contents of the JSON
                File.WriteAllText(outputJson, Spotify.ExportSpotifyJson(filteredEntries));

                Console.WriteLine($"[INFO] JSON file, '{Path.GetFileName(outputJson)}', generated successfully!");
            }

            Console.WriteLine($"[INFO] GPX file, '{Path.GetFileName(outputGpx)}', generated successfully.");
        }
        else if (args.Length == 1 && ".gpx" == Path.GetExtension(args[0]) || ".m3u" == Path.GetExtension(args[0]))
        {
            string inputFile = args[0];

            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"[INFO] Source {Path.GetExtension(args[0])} file, '{Path.GetFileName(inputFile)}', does not exist!");
                return;
            }

            // Convert GPX song points to m3u playlist
            if (".gpx" == Path.GetExtension(args[0]))
            {
                // Stage an output path for the resulting M3U file
                string outputPlist = Spotify.GenerateOutputPath(inputFile, "m3u");
                
                Console.WriteLine("[INFO] Convert GPX to m3u");
            }
            else if (".m3u" == Path.GetExtension(args[0]))
            {
                // Stage an output path for the resulting GPX file
                string outputGpx = Spotify.GenerateOutputPath(inputFile, "gpx");                
                
                Console.WriteLine("[INFO] Convert m3u to GPX");
            }
        }
        else
        {
            // None of these

            Console.WriteLine("[ERROR] Usage: SpotifyGPX [<json> <gpx>] [<json> <json>] [<gpx>] [<m3u>]");
            return;
        }

        // Exit the program
        return;
    }
}

class Spotify
{
    public static List<SpotifyEntry> FilterSpotifyJson(List<SpotifyEntry> spotifyEntries, List<GPXPoint> gpxPoints)
    {
        // Find the start and end times in GPX
        DateTimeOffset gpxStartTime = gpxPoints.Min(point => point.Time);
        DateTimeOffset gpxEndTime = gpxPoints.Max(point => point.Time);

        // Filter Spotify entries within the GPX timeframe
        List<SpotifyEntry> spotifyEntryCandidates = spotifyEntries
            .Where(entry => entry.Time_End >= gpxStartTime && entry.Time_End <= gpxEndTime)
            .ToList();

        return spotifyEntryCandidates;
    }

    public static List<(SpotifyEntry, GPXPoint)> CorrelateGpxPoints(List<SpotifyEntry> filteredEntries, List<GPXPoint> gpxPoints)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<(SpotifyEntry, GPXPoint)> correlatedEntries = new();

        foreach (SpotifyEntry spotifyEntry in filteredEntries)
        {
            GPXPoint nearestPoint = gpxPoints.OrderBy(point => Math.Abs((point.Time - spotifyEntry.Time_End).TotalSeconds)).First();
            correlatedEntries.Add((spotifyEntry, nearestPoint));
            Console.WriteLine($"[INFO] Entry Identified: '{Options.Identifier(spotifyEntry, new TimeSpan(), "name")}'");
        }

        return correlatedEntries;
    }

    public static string ExportSpotifyJson(List<SpotifyEntry> filteredEntries)
    {
        // Create a JSON document based on the list of songs within range
        string document = JsonConvert.SerializeObject(filteredEntries, Newtonsoft.Json.Formatting.Indented);

        return document;
    }

    public static string GenerateOutputPath(string inputFile, string format)
    {
        // Set up the output file path
        string outputFile = Path.Combine(Directory.GetParent(inputFile).ToString(), $"{Path.GetFileNameWithoutExtension(inputFile)}_Spotify.{format}");

        return outputFile;
    }
}

class GPX
{
    public static List<GPXPoint> ParseGPXFile(string gpxFilePath)
    {
        // Create a list of all GPX <trkpt> latitudes, longitudes, and times

        XDocument gpxDocument = XDocument.Load(gpxFilePath);
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        List<GPXPoint> gpxPoints = gpxDocument.Descendants(ns + "trkpt")
        .Select(trkpt => new GPXPoint
        {
            Time = DateTimeOffset.ParseExact(trkpt.Element(ns + "time").Value, Options.gpxPointTimeInp, null),
            Latitude = double.Parse(trkpt.Attribute("lat").Value),
            Longitude = double.Parse(trkpt.Attribute("lon").Value)
        })
        .ToList();

        // Return the list of points from the GPX
        return gpxPoints;
    }

    public static XmlDocument CreateGPXFile(List<(SpotifyEntry, GPXPoint)> finalPoints, string gpxFilePath)
    {
        // Create a new GPX document
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
        XmlElement gpxname = document.CreateElement("name");
        gpxname.InnerText = gpxFilePath;
        GPX.AppendChild(gpxname);

        // Initialize variable to count the number of songs added
        double songCount = 0;

        foreach ((SpotifyEntry song, GPXPoint point) in finalPoints)
        {
            // Create waypoint for each song
            XmlElement waypoint = document.CreateElement("wpt");
            GPX.AppendChild(waypoint);

            // Set the lat and lon of the waypoing to the original point
            waypoint.SetAttribute("lat", point.Latitude.ToString());
            waypoint.SetAttribute("lon", point.Longitude.ToString());

            // Set the name of the GPX point to the name of the song
            XmlElement name = document.CreateElement("name");
            name.InnerText = Options.Identifier(song, point.Time.Offset, "name");
            waypoint.AppendChild(name);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = point.Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            waypoint.AppendChild(time);

            // Set the description of the point 
            XmlElement description = document.CreateElement("desc");
            description.InnerText = Options.Identifier(song, point.Time.Offset, "desc");
            waypoint.AppendChild(description);

            songCount++;
        }

        Console.WriteLine($"[INFO] {songCount} songs written to GPX!");

        return document;
    }
}