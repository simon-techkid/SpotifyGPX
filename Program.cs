// SpotifyGPX by Simon Field
    
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length >= 2 && ".json" == Path.GetExtension(args[0]) && ".gpx" == Path.GetExtension(args[1]))
        {
            string inputJson = args[0];
            string inputGpx = args[1];
            bool noGpxExport = args.Length >= 3 && args.Contains("-n");
            bool exportJson = args.Length >= 3 && args.Contains("-j");
            bool exportPlist = args.Length >= 3 && args.Contains("-p");

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

            string outputGpx = GenerateOutputPath(inputGpx, "gpx");

            // Step 1: Create a list of all Spotify songs in the given JSON file
            List<SpotifyEntry> spotifyEntries;

            // Create a bool determining the Spotify JSON format used
            bool spotifyMiniJson;

            // Step 2: Create a list of all GPX points in the given GPX file
            List<GPXPoint> gpxPoints;

            // Step 3: Create a list of songs within the timeframe between the first and last GPX point
            List<SpotifyEntry> filteredEntries;

            // Step 4: Create a list of paired songs and points based on the closest time between each song and each GPX point
            List<(SpotifyEntry, GPXPoint)> correlatedEntries;

            try
            {
                // Step 1: Create list of songs contained in the JSON file
                (spotifyEntries, spotifyMiniJson) = JSON.ParseSpotifyJson(inputJson);

                // Step 2: Create list of GPX points from the GPX file
                gpxPoints = GPX.ParseGPXFile(inputGpx);

                // Step 3: Create list of songs played during the GPX tracking timeframe
                filteredEntries = JSON.FilterSpotifyJson(spotifyEntries, gpxPoints);

                // Step 4: Create list of songs and points paired as close as possible to one another
                correlatedEntries = GPX.CorrelateGpxPoints(filteredEntries, gpxPoints);
            }
            catch (Exception ex)
            {
                // Catch any errors found in the calculation process
                Console.WriteLine(ex);
                return;
            }

            Console.WriteLine($"[INFO] {filteredEntries.Count} Spotify entries filtered from {spotifyEntries.Count} total");
            Console.WriteLine($"[INFO] {correlatedEntries.Count} Spotify entries matched to set of {filteredEntries.Count}");

            if (noGpxExport == false)
            {
                // Create a GPX document based on the list of songs and points
                XmlDocument document = GPX.CreateGPXFile(correlatedEntries, inputGpx);

                // Write the contents of the GPX
                document.Save(outputGpx);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputGpx)} file, '{Path.GetFileName(outputGpx)}', generated successfully!");
            }

            if (exportJson == true)
            {
                // Stage output path of output JSON
                string outputJson = GenerateOutputPath(inputGpx, "json");

                // Write the contents of the JSON
                File.WriteAllText(outputJson, JSON.ExportSpotifyJson(filteredEntries, spotifyMiniJson));

                Console.WriteLine($"[INFO] {Path.GetExtension(outputJson)} file, '{Path.GetFileName(outputJson)}', generated successfully!");
            }

            if (exportPlist == true)
            {
                // Stage output path of output XSPF
                string outputPlist = GenerateOutputPath(inputGpx, "xspf");

                // Create an XML document for the playlist
                XmlDocument playlist = XSPF.CreatePlist(filteredEntries, outputPlist);

                // Write the contents of the XSPF
                playlist.Save(outputPlist);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputPlist)} file, {Path.GetFileName(outputPlist)}', generated successfully!");
            }
        }
        else
        {
            // None of these

            Console.WriteLine("[ERROR] Usage: SpotifyGPX <json> <gpx> [-j] [-p] [-n]");
            return;
        }

        // Exit the program
        return;
    }

    static string GenerateOutputPath(string inputFile, string format)
    {
        // Set up the output file path
        string outputFile = Path.Combine(Directory.GetParent(inputFile).ToString(), $"{Path.GetFileNameWithoutExtension(inputFile)}_Spotify.{format}");

        return outputFile;
    }
}

class JSON
{
    public static (List<SpotifyEntry>, bool) ParseSpotifyJson(string jsonFile)
    {
        // Create list of JSON objects
        List<JObject> jObjects = new();

        // Create variables to store a list of children counts for each JSON object and the average of all items in the list
        List<int> childrenCounts = new();
        double avgChildren = new();

        // Create list to store the parsed Spotify songs
        List<SpotifyEntry> spotifyEntries = new();

        try
        {
            // Attempt to deserialize JSON file to list
            jObjects = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFile));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deserializing given JSON file: {ex}");
        }

        try
        {
            // List how many children for each JSON object
            childrenCounts = jObjects.Select(jObject => jObject.Properties().Count()).ToList();

            // Find the average number of children among the entire JSON file
            avgChildren = Queryable.Average(childrenCounts.AsQueryable());
        }
        catch
        {
            throw new Exception($"Error calculating average size of JSON children!");
        }

        // Assume the Spotify JSON isn't formatted as "Extended Streaming History" (verbose)
        bool spotifyMiniJson = true;

        // Determine the format used for JSON in question
        if (avgChildren == 4)
        {
            spotifyMiniJson = true;
        }
        else if (avgChildren == 21)
        {
            spotifyMiniJson = false;
        }
        else
        {
            throw new Exception("Spotify JSON format invalid, not the correct number of children!");
        }

        try
        {
            spotifyEntries = jObjects.Select(jObject => new SpotifyEntry
            {
                Time_End = DateTimeOffset.ParseExact((string?)jObject[spotifyMiniJson ? "endTime" : "ts"], spotifyMiniJson ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                Spotify_Username = (string?)jObject["username"],
                Spotify_Platform = (string?)jObject["platform"],
                Time_Played = (string?)jObject[spotifyMiniJson ? "msPlayed" : "ms_played"],
                Spotify_Country = (string?)jObject["conn_country"],
                Spotify_IP = (string?)jObject["ip_addr_decrypted"],
                Spotify_UA = (string?)jObject["user_agent_decrypted"],
                Song_Name = (string?)jObject[spotifyMiniJson ? "trackName" : "master_metadata_track_name"],
                Song_Artist = (string?)jObject[spotifyMiniJson ? "artistName" : "master_metadata_album_artist_name"],
                Song_Album = (string?)jObject["master_metadata_album_album_name"],
                Song_URI = (string?)jObject["spotify_track_uri"],
                Episode_Name = (string?)jObject["episode_name"],
                Episode_Show = (string?)jObject["episode_show_name"],
                Episode_URI = (string?)jObject["spotify_episode_uri"],
                Song_StartReason = (string?)jObject["reason_start"],
                Song_EndReason = (string?)jObject["reason_end"],
                Song_Shuffle = (string?)jObject["shuffle"],
                Song_Skipped = (string?)jObject["skipped"],
                Spotify_Offline = (string?)jObject["offline"],
                Spotify_OfflineTS = (string?)jObject["offline_timestamp"],
                Spotify_Incognito = (string?)jObject["incognito"]
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing contents of JSON to a valid song entry: {ex}");
        }

        return (spotifyEntries, spotifyMiniJson);
    }

    public static List<SpotifyEntry> FilterSpotifyJson(List<SpotifyEntry> spotifyEntries, List<GPXPoint> gpxPoints)
    {
        // Find the start and end times in GPX
        DateTimeOffset gpxStartTime = gpxPoints.Min(point => point.Time);
        DateTimeOffset gpxEndTime = gpxPoints.Max(point => point.Time);

        // Create list of Spotify songs covering the tracked GPX path timeframe
        List<SpotifyEntry> spotifyEntryCandidates = new();

        try
        {
            // Attempt to filter Spotify entries within the GPX timeframe
            spotifyEntryCandidates = spotifyEntries
            .Where(entry => entry.Time_End >= gpxStartTime && entry.Time_End <= gpxEndTime)
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex}");
        }

        return spotifyEntryCandidates;
    }

    public static string ExportSpotifyJson(List<SpotifyEntry> tracks, bool spotifyMiniJson)
    {
        // Create a list of JSON objects
        List<JObject> json = new();

        if (spotifyMiniJson)
        {
            foreach (SpotifyEntry entry in tracks)
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["endTime"] = entry.Time_End.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                    ["artistName"] = entry.Song_Artist,
                    ["trackName"] = entry.Song_Name,
                    ["msPlayed"] = entry.Time_Played
                };

                json.Add(songEntry);
            }
        }
        else if (!spotifyMiniJson)
        {
            foreach (SpotifyEntry entry in tracks)
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["ts"] = entry.Time_End.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                    ["username"] = entry.Spotify_Username,
                    ["platform"] = entry.Spotify_Platform,
                    ["ms_played"] = entry.Time_Played,
                    ["conn_country"] = entry.Spotify_Country,
                    ["ip_addr_decrypted"] = entry.Spotify_IP,
                    ["user_agent_decrypted"] = entry.Spotify_UA,
                    ["master_metadata_track_name"] = entry.Song_Name,
                    ["master_metadata_album_artist_name"] = entry.Song_Artist,
                    ["master_metadata_album_album_name"] = entry.Song_Album,
                    ["spotify_track_uri"] = entry.Episode_URI,
                    ["episode_name"] = entry.Episode_Name,
                    ["episode_show_name"] = entry.Episode_Show,
                    ["spotify_episode_uri"] = entry.Episode_URI,
                    ["reason_start"] = entry.Song_StartReason,
                    ["reason_end"] = entry.Song_EndReason,
                    ["shuffle"] = entry.Song_Shuffle,
                    ["skipped"] = entry.Song_Skipped,
                    ["offline"] = entry.Spotify_Offline,
                    ["offline_timestamp"] = entry.Spotify_OfflineTS,
                    ["incognito"] = entry.Spotify_Incognito
                };

                json.Add(songEntry);
            }
        }

        // Create a JSON document based on the list of songs within range
        string document = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);

        return document;
    }
}

class GPX
{
    public static List<GPXPoint> ParseGPXFile(string gpxFile)
    {
        // Create a new XML document
        XDocument document = new();
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        // Create a list of interpreted GPX points
        List<GPXPoint> gpxPoints = new();

        try
        {
            // Attempt to load the contents of the specified file into the XML
            document = XDocument.Load(gpxFile);
        }
        catch (Exception ex)
        {
            // If the specified XML is invalid, throw an error
            throw new Exception($"The defined GPX file is incorrectly formatted: {ex}");
        }

        if (!document.Descendants(ns + "trkpt").Any())
        {
            // If there are no <trkpt> point elements in the GPX, throw an error
            throw new Exception($"No points found in '{Path.GetFileName(gpxFile)}'!");
        }

        try
        {
            // Attempt to add all GPX <trkpt> latitudes, longitudes, and times to the gpxPoints list
            gpxPoints = document.Descendants(ns + "trkpt")
            .Select(trkpt => new GPXPoint
            {
                Time = DateTimeOffset.ParseExact(trkpt.Element(ns + "time").Value, Options.gpxPointTimeInp, null),
                Latitude = double.Parse(trkpt.Attribute("lat").Value),
                Longitude = double.Parse(trkpt.Attribute("lon").Value)
            })
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"The GPX parameters cannot be parsed: {ex}");
        }

        // Return the list of points from the GPX
        return gpxPoints;
    }

    public static List<(SpotifyEntry, GPXPoint)> CorrelateGpxPoints(List<SpotifyEntry> filteredEntries, List<GPXPoint> gpxPoints)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<(SpotifyEntry, GPXPoint)> correlatedEntries = new();

        // Create variable to count how many songs are included
        double songCount = 0;

        // Create a list of correlation accuracies, one for each song
        List<double> correlationAccuracy = new();

        foreach (SpotifyEntry spotifyEntry in filteredEntries)
        {
            // Create variable to hold the calculated nearest GPX point to each song
            GPXPoint nearestPoint = new();

            try
            {
                // Find nearest GPX point using smallest possible absolute value with GPX and song end times
                nearestPoint = gpxPoints.OrderBy(point => Math.Abs((point.Time - spotifyEntry.Time_End).TotalSeconds)).First();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error ordering point and song times: {ex}");
            }

            // Set the accuracy value to the absolute value in seconds between the GPX and song end times
            double accuracySec = Math.Abs((nearestPoint.Time - spotifyEntry.Time_End).TotalSeconds);

            // Add correlation accuracy (seconds) to the correlation accuracies list
            correlationAccuracy.Add(accuracySec);

            // Add both the current Spotify entry and calculated nearest point to the correlated entries list
            correlatedEntries.Add((spotifyEntry, nearestPoint));

            // Add one to the number of songs counted
            songCount++;

            Console.WriteLine($"[SONG] [{songCount}] [{accuracySec} sec] ==> '{Options.Identifier(spotifyEntry, nearestPoint.Time.Offset, "name")}'");
        }

        if (correlatedEntries.Count < 1)
        {
            throw new Exception("No entries found to add!");
        }

        // Calculate and print the average correlation accuracy in seconds
        Console.WriteLine($"[INFO] Song-Point Correlation Accuracy (avg sec): {Math.Round(Queryable.Average(correlationAccuracy.AsQueryable()))}");

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }

    public static XmlDocument CreateGPXFile(List<(SpotifyEntry, GPXPoint)> finalPoints, string gpxFile)
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
        gpxname.InnerText = Path.GetFileName(gpxFile);
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

        Console.WriteLine($"[INFO] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFile)}' added to GPX.");

        return document;
    }
}

class XSPF
{
    public static XmlDocument CreatePlist(List<SpotifyEntry> tracks, string plistFile)
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
        name.InnerText = Path.GetFileNameWithoutExtension(plistFile);
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