// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string progMode = "gpx";
        
        if (args.Length != 2)
        {
            // if there are not two arguments
            if (args.Length == 3)
            {
                // if there is a third argument
                progMode = args[2];
            }
            else
            {
                // if there is no third argument
                Console.WriteLine("[INFO] Valid arguments: <json_file> <gpx_file> [prog_mode]");
                return;
            }
        }

        string spotifyJsonPath = args[0]; // Spotify JSON file path
        string gpxFilePath = args[1]; // Input GPX file path

        if (progMode != "gpx")
        {
            if (progMode != "json" && progMode != "plist")
            {
                Console.WriteLine("[INFO] Valid program modes: [gpx], [json], [plist]");
                return;
            }
        }

        if (!File.Exists(spotifyJsonPath))
        {
            // Specified JSON does not exist at the specified path
            Console.WriteLine($"[ERROR] Source Spotify JSON is not found!");
            return;
        }
        else if (!Path.HasExtension(".json"))
        {
            // Specified JSON does not carry JSON extension
            Console.WriteLine($"[ERROR] Source Spotify JSON does not use JSON extension!");
            return;
        }

        if (!File.Exists(gpxFilePath))
        {
            // Specified GPX does not exist at the specified path
            Console.WriteLine($"[ERROR] Source GPX file, '{Path.GetFileName(gpxFilePath)}', is not found!");
            return;
        }
        else if (!Path.HasExtension(".gpx"))
        {
            // Specified GPX does not carry GPX extension
            Console.WriteLine($"[ERROR] Source GPX file, '{Path.GetFileName(gpxFilePath)}', does not use GPX extension!");
            return;
        }

        List<SpotifyEntry> allSpotifyEntries = new();

        try
        {
            // Load Spotify JSON data
            allSpotifyEntries = JsonConvert.DeserializeObject<List<SpotifyEntry>>(File.ReadAllText(spotifyJsonPath));
        }
        catch (Exception ex)
        {
            // Handle parsing errors here
            Console.WriteLine($"[ERROR] Problem parsing JSON file: {ex.Message}");
            return;
        }        

        // Load GPX data
        List<GPXPoint> gpxPoints = ParseGPXFile(gpxFilePath);

        // Find the start and end times in GPX
        DateTimeOffset gpxStartTime = gpxPoints.Min(point => point.Time);
        DateTimeOffset gpxEndTime = gpxPoints.Max(point => point.Time);

        // Filter Spotify entries within the GPX timeframe
        List<SpotifyEntry> spotifyEntryCandidates = allSpotifyEntries
            .Where(entry => ReadJsonTime(entry.endTime) >= gpxStartTime && ReadJsonTime(entry.endTime) <= gpxEndTime)
            .ToList();

        // Correlate Spotify entries with the nearest GPX points
        List<(SpotifyEntry, GPXPoint)> correlatedEntries = new();
        foreach (SpotifyEntry spotifyEntry in spotifyEntryCandidates)
        {
            GPXPoint nearestPoint = gpxPoints.OrderBy(point => Math.Abs((point.Time - ReadJsonTime(spotifyEntry.endTime)).TotalSeconds)).First();
            correlatedEntries.Add((spotifyEntry, nearestPoint));
            Console.WriteLine($"[INFO] Entry Identified: '{Options.Identifier(spotifyEntry, "name")}'");
        }

        if (progMode == "json")
        {            
            // Set up the output file path
            string jsonFileOut = Path.Combine(Directory.GetParent(gpxFilePath).ToString(), $"{Path.GetFileNameWithoutExtension(gpxFilePath)}_Spotify.json");

            if (File.Exists(jsonFileOut))
            {
                // If the outgoing file would be overwritten, abort:
                Console.WriteLine($"[INFO] Target JSON file, '{Path.GetFileName(jsonFileOut)}', already exists!");
                return;
            }
            else
            {
                // Create a JSON document based on the list of songs within range
                string document = JsonConvert.SerializeObject(spotifyEntryCandidates, Newtonsoft.Json.Formatting.Indented);

                // Save the JSON to a file
                File.WriteAllText(jsonFileOut, document);

                // Send success message
                Console.WriteLine($"[INFO] JSON file, '{Path.GetFileName(jsonFileOut)}', generated successfully.");
            }
        }
        else if (progMode == "plist")
        {

        }
        else if (progMode == "gpx")
        {
            // Set up the output file path
            string gpxFileOut = Path.Combine(Directory.GetParent(gpxFilePath).ToString(), $"{Path.GetFileNameWithoutExtension(gpxFilePath)}_Spotify.gpx");

            if (File.Exists(gpxFileOut))
            {
                // If the outgoing file would be overwritten, abort:
                Console.WriteLine($"[ERROR] Target GPX file, '{Path.GetFileName(gpxFileOut)}', already exists!");
                return;
            }
            else
            {
                // Create a GPX document based on the list of points
                XmlDocument document = CreateGPXFile(correlatedEntries, Path.GetFileName(gpxFilePath));

                // Save the GPX to the file
                document.Save(gpxFileOut);

                // Send success message
                Console.WriteLine($"[INFO] GPX file, '{Path.GetFileName(gpxFileOut)}', generated successfully.");
            }           
        }

        // Exit the program
        return;
    }

    public static DateTimeOffset ReadJsonTime(string? inputTime)
    {
        DateTimeOffset spotifyTimestamp = DateTimeOffset.ParseExact(inputTime, Options.spotifyJsonTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        return spotifyTimestamp;        
    }

    public static List<GPXPoint> ParseGPXFile(string gpxFilePath)
    {
        // Create a list of all GPX <trkpt> latitudes, longitudes, and times

        XDocument gpxDocument = XDocument.Load(gpxFilePath);
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        List<GPXPoint> gpxPoints = gpxDocument.Descendants(ns + "trkpt")
        .Select(trkpt => new GPXPoint
        {
            Time = DateTimeOffset.ParseExact(trkpt.Element(ns + "time").Value, Options.gpxPointTimeInp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
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
            name.InnerText = Options.Identifier(song, "name");
            waypoint.AppendChild(name);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = point.Time.ToString(Options.gpxPointTimeOut);
            waypoint.AppendChild(time);

            // Set the description of the point to that defined in options
            XmlElement description = document.CreateElement("desc");
            description.InnerText = Options.Identifier(song, "desc");
            waypoint.AppendChild(description);

            songCount++;
        }

        Console.WriteLine($"[INFO] {songCount} songs written to GPX!");

        return document;
    }
}