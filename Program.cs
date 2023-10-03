// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using SpotifyGPX;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length >= 2 && ".json" == Path.GetExtension(args[0]) && ".gpx" == Path.GetExtension(args[1]))
        {
            string inputJson = args[0];
            string inputGpx = args[1];
            bool exportJson = args.Length >= 3 && args.Contains("-j");
            bool exportPlist = args.Length >= 3 && args.Contains("-p");
            bool noGpxExport = args.Length >= 3 && args.Contains("-n");

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

            string outputGpx = Spotify.GenerateOutputPath(inputGpx, "gpx");

            // Create a list of all Spotify songs in the given JSON file
            (List<SpotifyEntry> spotifyEntries, bool spotifyMiniJson) = JSON.ParseSpotifyJson(inputJson);

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

            if (noGpxExport ==  false)
            {
                // Create a GPX document based on the list of songs and points
                XmlDocument document = GPX.CreateGPXFile(correlatedEntries, Path.GetFileName(inputGpx));

                // Save the GPX to the file
                document.Save(outputGpx);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputGpx)} file, '{Path.GetFileName(outputGpx)}', generated successfully!");
            }

            if (exportJson == true)
            {
                string outputJson = Spotify.GenerateOutputPath(inputGpx, "json");

                // Write the contents of the JSON
                File.WriteAllText(outputJson, JSON.ExportSpotifyJson(filteredEntries, spotifyMiniJson));

                Console.WriteLine($"[INFO] {Path.GetExtension(outputJson)} file, '{Path.GetFileName(outputJson)}', generated successfully!");
            }

            if (exportPlist == true)
            {
                string outputPlist = Spotify.GenerateOutputPath(inputGpx, "xspf");

                XmlDocument playlist = XSPF.CreatePlist(filteredEntries, outputPlist);

                playlist.Save(outputPlist);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputPlist)} file, {Path.GetFileName(outputPlist)}', generated successfully!");
            }
        }
        else
        {
            // None of these

            Console.WriteLine("[ERROR] Usage: SpotifyGPX <json> <gpx> [-j] [-p]");
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

class JSON
{
    public static (List<SpotifyEntry>, bool) ParseSpotifyJson(string inputJson)
    {
        List<JObject> jObjects = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(inputJson));

        // Create a list of how many children for each JSON object
        List<int> childrenCounts = jObjects.Select(jObject => jObject.Properties().Count()).ToList();

        // Find the average number of children among the entire JSON file
        double children = Queryable.Average(childrenCounts.AsQueryable());

        // Assume the Spotify JSON isn't formatted as "Extended Streaming History" (verbose)
        bool spotifyMiniJson = true;

        // Determine the format used for JSON in question
        if (children == 4)
        {
            spotifyMiniJson = true;
        }
        else if (children == 21)
        {
            spotifyMiniJson = false;
        }
        else
        {
            throw new Exception("Spotify JSON format unrecognized, not the correct number of children!");
        }

        List<SpotifyEntry> spotifyEntries = jObjects.Select(jObject => new SpotifyEntry
        {
            Time_End = DateTimeOffset.ParseExact((string?)jObject[spotifyMiniJson ? "endTime" : "ts"], spotifyMiniJson ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            Spotify_Username = spotifyMiniJson ? null : (string?)jObject["username"],
            Spotify_Platform = spotifyMiniJson ? null : (string?)jObject["platform"],
            Time_Played = (string?)jObject[spotifyMiniJson ? "msPlayed" : "ms_played"],
            Spotify_Country = spotifyMiniJson ? null : (string?)jObject["conn_country"],
            Spotify_IP = spotifyMiniJson ? null : (string?)jObject["ip_addr_decrypted"],
            Spotify_UA = spotifyMiniJson ? null : (string?)jObject["user_agent_decrypted"],
            Song_Name = (string?)jObject[spotifyMiniJson ? "trackName" : "master_metadata_track_name"],
            Song_Artist = (string?)jObject[spotifyMiniJson ? "artistName" : "master_metadata_album_artist_name"],
            Song_Album = spotifyMiniJson ? null : (string?)jObject["master_metadata_album_album_name"],
            Song_URI = spotifyMiniJson ? null : (string?)jObject["spotify_track_uri"],
            Episode_Name = spotifyMiniJson ? null : (string?)jObject["episode_name"],
            Episode_Show = spotifyMiniJson ? null : (string?)jObject["episode_show_name"],
            Episode_URI = spotifyMiniJson ? null : (string?)jObject["spotify_episode_uri"],
            Song_StartReason = spotifyMiniJson ? null : (string?)jObject["reason_start"],
            Song_EndReason = spotifyMiniJson ? null : (string?)jObject["reason_end"],
            Song_Shuffle = spotifyMiniJson ? null : (string?)jObject["shuffle"],
            Song_Skipped = spotifyMiniJson ? null : (string?)jObject["skipped"],
            Spotify_Offline = spotifyMiniJson ? null : (string?)jObject["offline"],
            Spotify_OfflineTS = spotifyMiniJson ? null : (string?)jObject["offline_timestamp"],
            Spotify_Incognito = spotifyMiniJson ? null : (string?)jObject["incognito"]
        }).ToList();

        return (spotifyEntries, spotifyMiniJson);
    }

    public static string ExportSpotifyJson(List<SpotifyEntry> tracks, bool spotifyMiniJson)
    {
        // Create a list of JSON objects
        List<JObject> json = new();

        foreach (SpotifyEntry entry in tracks)
        {
            // Create a JSON object containing each element of a SpotifyEntry
            JObject songEntry = new()
            {
                ["endTime"] = entry.Time_End.ToString(spotifyMiniJson ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                ["artistName"] = entry.Song_Artist,
                ["trackName"] = entry.Song_Name,
                ["msPlayed"] = entry.Time_Played
            };

            json.Add(songEntry);
        }

        // Create a JSON document based on the list of songs within range
        string document = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);

        return document;
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

class XSPF
{
    public static List<SpotifyEntry> ParsePlist(string plistFilePath)
    {
        // Create an XML document based on the file path
        XDocument document = XDocument.Load(plistFilePath);
        XNamespace ns = "http://xspf.org/ns/0/";

        // Create a list of all GPX <trkpt> latitudes, longitudes, and times
        List<SpotifyEntry> tracks = document.Descendants(ns + "track")
        .Select(track => new SpotifyEntry
        {
            Time_End = DateTimeOffset.ParseExact(track.Element(ns + "annotation").Value, Options.gpxPointTimeInp, null),
            Song_Artist = track.Attribute("creator").Value,
            Song_Name = track.Attribute("title").Value,
            Time_Played = track.Attribute("duration").Value
        })
        .ToList();

        return tracks;
    }

    public static XmlDocument CreatePlist(List<SpotifyEntry> tracks, string plistFilePath)
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

        // Set the name of the XSPF playlist to the name of the file
        XmlElement name = document.CreateElement("name");
        name.InnerText = Path.GetFileNameWithoutExtension(plistFilePath);
        XSPF.AppendChild(name);

        // Set the title of the XSPF playlist to the name of the file
        XmlElement creator = document.CreateElement("creator");
        creator.InnerText = "SpotifyGPX";
        XSPF.AppendChild(creator);

        // Create the trackList header
        XmlElement trackList = document.CreateElement("trackList");
        XSPF.AppendChild(trackList);

        foreach (SpotifyEntry entry in tracks)
        {
            // Create track for each song
            XmlElement track = document.CreateElement("track");
            trackList.AppendChild(track);

            // Set the creator of the track to the song artist
            XmlElement artist = document.CreateElement("creator");
            artist.InnerText = entry.Song_Artist;
            track.AppendChild(artist);

            // Set the title of the track to the song name
            XmlElement title = document.CreateElement("title");
            title.InnerText = entry.Song_Name;
            track.AppendChild(title);

            // Set the annotation of the song to the end time
            XmlElement annotation = document.CreateElement("annotation");
            annotation.InnerText = entry.Time_End.ToString(Options.gpxPointTimeInp);
            track.AppendChild(annotation);

            // Set the duration of the song to the amount of time it was listened to
            XmlElement duration = document.CreateElement("duration");
            duration.InnerText = entry.Time_Played;
            track.AppendChild(duration);
        }

        return document;
    }
}