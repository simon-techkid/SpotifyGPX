// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using SpotifyGPX;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length >= 2 && ".json" == Path.GetExtension(args[0]) && ".gpx" == Path.GetExtension(args[1]))
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
            List<SpotifyEntry> spotifyEntries = JSON.ParseSpotifyJson(inputJson);

            // Create a list of all GPX points in the given GPX file
            List<GPXPoint> gpxPoints = GPX.ParseGPXFile(inputGpx);

            Console.WriteLine($"[INFO] {gpxPoints.Count} GPX points loaded!");

            // Create a list of songs within the timeframe between the first and last GPX point
            List<SpotifyEntry> filteredEntries = Spotify.FilterSpotifyJson(spotifyEntries, gpxPoints);

            // Create a list of paired songs and points based on the closest time between each song and each GPX point
            (List<(SpotifyEntry, GPXPoint)> correlatedEntries, List<double> correlationAccuracy) = Spotify.CorrelateGpxPoints(filteredEntries, gpxPoints);

            Console.WriteLine($"[INFO] {filteredEntries.Count} Spotify entries filtered from {spotifyEntries.Count} total");
            Console.WriteLine($"[INFO] {correlatedEntries.Count} Spotify entries matched to set of {filteredEntries.Count}");

            if (correlatedEntries.Count < 1)
            {
                Console.WriteLine("[ERROR] No entries found to add!");
                return;
            }

            // Calculate and print the average correlation accuracy in seconds
            Console.WriteLine($"[INFO] Song-Point Correlation Accuracy (avg sec): {Math.Round(Queryable.Average(correlationAccuracy.AsQueryable()))}");

            // Create a GPX document based on the list of songs and points
            XmlDocument document = GPX.CreateGPXFile(correlatedEntries, Path.GetFileName(inputGpx));

            // Save the GPX to the file
            document.Save(outputGpx);

            if (exportJson == true)
            {
                // Stage an output path for the resulting JSON file
                string outputJson = Spotify.GenerateOutputPath(inputGpx, "json");

                // Write the contents of the JSON
                File.WriteAllText(outputJson, JSON.ExportSpotifyJson(filteredEntries));

                Console.WriteLine($"[INFO] JSON file, '{Path.GetFileName(outputJson)}', generated successfully!");
            }

            Console.WriteLine($"[INFO] GPX file, '{Path.GetFileName(outputGpx)}', generated successfully!");
        }
        else if (args.Length == 1 && (".gpx" == Path.GetExtension(args[0]) || ".m3u" == Path.GetExtension(args[0])))
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

            Console.WriteLine("[ERROR] Usage: SpotifyGPX [<json> <gpx> [-j]] [<gpx>] [<m3u>]");
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

    public static (List<(SpotifyEntry, GPXPoint)>, List<double>) CorrelateGpxPoints(List<SpotifyEntry> filteredEntries, List<GPXPoint> gpxPoints)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<(SpotifyEntry, GPXPoint)> correlatedEntries = new();

        double songCount = 0;
        List<double> correlationAccuracy = new();

        foreach (SpotifyEntry spotifyEntry in filteredEntries)
        {
            GPXPoint nearestPoint = gpxPoints.OrderBy(point => Math.Abs((point.Time - spotifyEntry.Time_End).TotalSeconds)).First();
            correlatedEntries.Add((spotifyEntry, nearestPoint));
            songCount++;
            correlationAccuracy.Add(Math.Abs((nearestPoint.Time - spotifyEntry.Time_End).TotalSeconds));
            Console.WriteLine($"[SONG] [{songCount}] ==> '{Options.Identifier(spotifyEntry, nearestPoint.Time.Offset, "name")}'");
        }

        return (correlatedEntries, correlationAccuracy);
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
        // Create an XML document based on the file path
        XDocument document = XDocument.Load(gpxFilePath);
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        // Create a list of all GPX <trkpt> latitudes, longitudes, and times
        List<GPXPoint> gpxPoints = document.Descendants(ns + "trkpt")
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
        XmlElement gpxname = document.CreateElement("name");
        gpxname.InnerText = gpxFilePath;
        GPX.AppendChild(gpxname);

        double pointCount = 0;

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
            pointCount++;
        }

        Console.WriteLine($"[INFO] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFilePath)}' added to GPX.");

        return document;
    }
}

class PLIST
{
    public static string ParsePlist(string plistFilePath)
    {
        // Create an XML document based on the file path
        XDocument document = XDocument.Load(plistFilePath);
        XNamespace ns = "http://xspf.org/ns/0/";

        return null;
    }

    public static XmlDocument CreatePlist(List<(SpotifyEntry, GPXPoint)> finalPoints, string plistFilePath)
    {
        // Create a new XML document
        XmlDocument document = new();

        // Create the XML header
        XmlNode header = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(header);

        // Create the XSPF header
        XmlElement XSPF = document.CreateElement("playlist");
        document.AppendChild(XSPF);

        // Add XSPF header attributes
        XSPF.SetAttribute("version", "1.0");
        XSPF.SetAttribute("xmlns", "http://xspf.org/ns/0/");

        // Set the title of the XSPF playlist to the name of the file
        XmlElement title = document.CreateElement("name");
        title.InnerText = Path.GetFileNameWithoutExtension(plistFilePath);
        XSPF.AppendChild(title);

        // Set the title of the XSPF playlist to the name of the file
        XmlElement creator = document.CreateElement("creator");
        creator.InnerText = "SpotifyGPX";
        XSPF.AppendChild(creator);

        // Create the trackList header
        XmlElement trackList = document.CreateElement("trackList");
        XSPF.AppendChild(trackList);


        return document;
    }
}