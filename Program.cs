// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpotifyGPX.Parsing;
using SpotifyGPX.Options;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("[INFO] Valid arguments: <json_file> <gpx_file>");
            return;
        }

        string spotifyJsonPath = args[0]; // Spotify JSON file path
        string gpxFilePath = args[1]; // Input GPX file path
        string finalFilePath; // Output GPX file path

        if (!File.Exists(spotifyJsonPath))
        {
            // Specified JSON does not exist at the specified path
            Console.WriteLine($"[ERROR] Source Spotify JSON is not found!");
            return;
        }
        else
        {
            // Specified JSON does exist
            if (!Path.HasExtension(".json"))
            {
                // Specified JSON does not carry JSON extension
                Console.WriteLine($"[ERROR] Source Spotify JSON does not use JSON extension!");
                return;
            }
        }

        if (!File.Exists(gpxFilePath))
        {
            // Specified GPX does not exist at the specified path
            Console.WriteLine($"[ERROR] Source GPX file, '{Path.GetFileName(gpxFilePath)}', is not found!");
            return;
        }
        else
        {
            // Specified GPX does exist
            if (!Path.HasExtension(".gpx"))
            {
                // Specified GPX does not carry GPX extension
                Console.WriteLine($"[ERROR] Source GPX file, '{Path.GetFileName(gpxFilePath)}', does not use GPX extension!");
                return;
            }
            else
            {
                // Specified GPX does carry GPX extension

                // Input GPX: 20230924.gpx
                // Resulting output GPX: 20230924_Spotify.gpx                
                finalFilePath = Path.Combine(Directory.GetParent(gpxFilePath).ToString(), $"{Path.GetFileNameWithoutExtension(gpxFilePath)}_Spotify.gpx");
                
                // Abort if there is already a GPX there
                if (File.Exists(finalFilePath))
                {
                    // If the outgoing GPX would be overwritten, abort:
                    Console.WriteLine($"[ERROR] Target GPX file, '{Path.GetFileName(finalFilePath)}', already exists!");
                    return;
                }
            }
        }

        List<SpotifyEntry> spotifyEntries = new();

        try
        {
            // Load Spotify JSON data
            spotifyEntries = JsonConvert.DeserializeObject<List<SpotifyEntry>>(File.ReadAllText(spotifyJsonPath));
        }
        catch (Exception ex)
        {
            // Handle parsing errors here
            Console.WriteLine($"[ERROR] Problem parsing JSON file: {ex.Message}");
            return;
        }        

        // Load GPX data
        List<GPXPoint> gpxPoints = GPX.ParseGPXFile(gpxFilePath);

        // Find the start and end times in GPX
        DateTimeOffset gpxStartTime = gpxPoints.Min(point => point.Time);
        DateTimeOffset gpxEndTime = gpxPoints.Max(point => point.Time);

        // Filter Spotify entries within the GPX timeframe
        List<SpotifyEntry> spotifyEntriesInRange = spotifyEntries
            .Where(entry => Spotify.JsonTimeZone(entry.endTime) >= gpxStartTime && Spotify.JsonTimeZone(entry.endTime) <= gpxEndTime)
            .ToList();

        // Correlate Spotify entries with the nearest GPX points
        List<(SpotifyEntry, GPXPoint)> correlatedEntries = new();
        foreach (SpotifyEntry spotifyEntry in spotifyEntriesInRange)
        {
            GPXPoint nearestPoint = gpxPoints.OrderBy(point => Math.Abs((point.Time - Spotify.JsonTimeZone(spotifyEntry.endTime)).TotalSeconds)).First();
            correlatedEntries.Add((spotifyEntry, nearestPoint));
            Console.WriteLine($"[INFO] JSON Entry Identified: '{SongResponse.Identifier(spotifyEntry, "name")}'");
        }

        // Display the correlated entries
        foreach (var (spotifyEntry, gpxPoint) in correlatedEntries)
        {
            Console.WriteLine($"Spotify Track: {spotifyEntry.trackName} by {spotifyEntry.artistName}");
            Console.WriteLine($"GPX Point Time: {gpxPoint.Time}, Lat: {gpxPoint.Latitude}, Lon: {gpxPoint.Longitude}");
            Console.WriteLine();
        }

        // OTHER FEATURES TO ADD:
        // - JSON exporting (export the relevant part of the Spotify JSON to a new file for future reference)
        // - Playlist exporting (export a GPX of song points to a m3u or some such file)
        // - Spotify linkage (export a series of spotify URI so these can be reimported
        
        // Create a GPX document based on the list of points
        XmlDocument document = GPX.CreateGPXFile(correlatedEntries, Path.GetFileName(gpxFilePath));

        // Save the GPX to the file
        document.Save(finalFilePath);

        // Exit the program
        Console.WriteLine($"[INFO] GPX file, '{Path.GetFileName(finalFilePath)}', generated successfully.");
        return;
    }
}

namespace SpotifyGPX.Parsing
{
    public static class Spotify
    {
        public static DateTimeOffset JsonTimeZone(string inputTime)
        {
            DateTimeOffset spotifyTimestamp = DateTimeOffset.ParseExact(inputTime, SongResponse.spotifyJsonTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            return spotifyTimestamp;
        }
    }

    public static class GPX
    {        
        public static List<GPXPoint> ParseGPXFile(string gpxFilePath)
        {
            // Create a list of all GPX <trkpt> latitudes, longitudes, and times

            XDocument gpxDocument = XDocument.Load(gpxFilePath);
            XNamespace ns = "http://www.topografix.com/GPX/1/0";

            List<GPXPoint> gpxPoints = gpxDocument.Descendants(ns + "trkpt")
            .Select(trkpt => new GPXPoint
            {
                Time = DateTimeOffset.ParseExact(trkpt.Element(ns + "time").Value, SongResponse.gpxPointTimeInp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                Latitude = double.Parse(trkpt.Attribute("lat").Value),
                Longitude = double.Parse(trkpt.Attribute("lon").Value)
            })
            .ToList();

            // Return the list of points from the GPX
            return gpxPoints;
        }

        public static XmlDocument CreateGPXFile(List<(SpotifyEntry, GPXPoint)> finalPoints, string gpxFile)
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
            gpxname.InnerText = gpxFile;
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
                name.InnerText = SongResponse.Identifier(song, "name");
                waypoint.AppendChild(name);

                // Set the time of the GPX point to the original time
                XmlElement time = document.CreateElement("time");
                time.InnerText = point.Time.ToString(SongResponse.gpxPointTimeOut);
                waypoint.AppendChild(time);

                // Set the description of the point to that defined in options
                XmlElement description = document.CreateElement("desc");
                description.InnerText = SongResponse.Identifier(song, "desc");
                waypoint.AppendChild(description);

                // Inform the user of the creation of GPX point
                Console.WriteLine($"[INFO] Created GPX Point: '{name.InnerText}'");
                songCount++;
            }

            Console.WriteLine($"[INFO] {songCount} songs written to GPX!");

            return document;
        }
    }
}

