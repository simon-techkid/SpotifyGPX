// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("[INFO] Valid arguments: <json_file> <gpx_file>");
            return;
        }

        string spotifyJsonPath = args[0]; // JSON file path
        string gpxFilePath = args[1]; // GPX file path
        string finalFilePath; // GPX file path

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

        // Load Spotify JSON data
        List<SpotifyEntry> spotifyDump = JsonConvert.DeserializeObject<List<SpotifyEntry>>(File.ReadAllText(spotifyJsonPath));

        // Load GPX data
        List<GPXPoint> gpxPoints = GPX.ParseGPXFile(gpxFilePath);

        // Create song variable to hold the previous song
        SpotifyEntry currentSong = null;

        // Create a variable to hold the nearest calculated song
        SpotifyEntry nearestSong = null;

        // Create a list of each task created
        List<Task> createdTasks = new();

        // Create a list of completed tasks, containing their resulting songs
        List<SpotifyEntry> completedTasks = new();

        // Create a list of songs and points to be used for the final GPX
        List<(SpotifyEntry, GPXPoint)> finalPoints = new();

        // Iterate through GPX points and find the nearest song
        foreach (var gpxPoint in gpxPoints)
        {
            // Find the nearest song based on timestamp
            Task<SpotifyEntry> task = Task.Run(() => Spotify.FindNearestSong(spotifyDump, gpxPoint.Time));

            // Add the task to the created tasks list
            createdTasks.Add(task);

            // Add the task's song to the completed tasks list once it is finished calculating
            completedTasks.Add(await task);

            // Set the nearest song calculated to the identified result
            nearestSong = task.Result;

            // NEW FEATURE:
            // Create list of all songs in the spotify json
            // Find the index of the first and last song in the spotify song list that is correlated to the GPX
            // If any are spotify SongEntry list items between the first and last index that are not sent to the GPX, warn the user (this assumes all songs listened to during the GPX period should be included in the GPX at all costs)
            


            // Ensures the identified song does not contain duplicate entries
            if (nearestSong != null && !Spotify.IsSameSong(nearestSong, currentSong))
            {
                // If this song is different to the prior:
                
                Console.WriteLine($"[INFO] JSON entry identified: '{SongResponse.Identifier(nearestSong, "name")}'");

                // Add the calculated nearest song and its point to the final list
                finalPoints.Add((nearestSong, gpxPoint));

                // Update the current song to avoid duplicates
                currentSong = nearestSong;
            }
        }

        // Wait for all nearest songs to be calculated and identified
        Task.WaitAll(createdTasks.ToArray());

        // Create a GPX document based on the list of points
        XmlDocument document = GPX.CreateGPXFile(finalPoints, Path.GetFileName(gpxFilePath));

        // Save the GPX to the file
        document.Save(finalFilePath);

        // Exit the program
        Console.WriteLine($"[INFO] GPX file, '{Path.GetFileName(finalFilePath)}', generated successfully.");
        return;
    }
}

public static class Spotify
{
    public static SpotifyEntry FindNearestSong(List<SpotifyEntry> spotifyData, DateTimeOffset trkptTimestamp)
    {
        // Initialize variables to keep track of the nearest song
        SpotifyEntry nearestSong = null;

        // Initialize variable to hold max time difference, starting at infinity working downward
        double nearestTimeDifference = double.MaxValue;

        // use this to keep track of last index of spotifyData list
        double lastIndex;

        // if this doesn't match last one, warn that a song was missed, and print the entry

        foreach (SpotifyEntry entry in spotifyData)
        {
            // For every entry in the Spotify JSON:

            // Parse the date and time when the song ended
            DateTime songEndTimestamp = DateTime.Parse(entry.ts);

            // Calculate the time difference in seconds between the GPX point timestamp and the song end timestamp
            double timeDifferenceSec = Math.Abs((songEndTimestamp - trkptTimestamp).TotalSeconds);

            // Check if this song is closer than the previous song to the GPX point
            if (timeDifferenceSec < nearestTimeDifference)
            {
                // If it is closer, continue:

                if (trkptTimestamp <= songEndTimestamp)
                {
                    // The GPX point was taken before the end of the song, it is valid:
                    
                    nearestSong = entry;
                    nearestTimeDifference = timeDifferenceSec;
                }
            }
            else
            {
                // If this song is farther than the previous (reader has passed relevant songs), skip it:
                
                break;
            }
        }

        // Return the calculated nearest song of the point
        return nearestSong;
    }

    public static bool IsSameSong(SpotifyEntry song1, SpotifyEntry song2)
    {
        // Check if two Spotify entries represent the same song
        return song1 != null && song2 != null && song1.master_metadata_track_name == song2.master_metadata_track_name;
    }
}

public static class GPX
{
    public static List<GPXPoint> ParseGPXFile(string filePath)
    {
        // Create a list of all GPX <trkpt> latitudes, longitudes, and times
        List<GPXPoint> gpxPoints = new();

        try
        {
            // Load the GPX
            XDocument gpxDoc = XDocument.Load(filePath);

            // Import the GPX 1.0 Namespace
            XNamespace gpxns = "http://www.topografix.com/GPX/1/0";

            foreach (XElement trackPoint in gpxDoc.Descendants(gpxns + "trkpt"))
            {
                // For every <trkpt> record:

                double latitude = (double)trackPoint.Attribute("lat"); // Latitude
                double longitude = (double)trackPoint.Attribute("lon"); // Longitude

                // Parse GPX timestamp including offset
                DateTimeOffset timestamp = DateTimeOffset.ParseExact(
                    trackPoint.Element(gpxns + "time").Value,
                    "yyyy-MM-ddTHH:mm:ss.fffzzz",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal
                );

                // Create GPX point data for that record
                GPXPoint gpxPoint = new()
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Time = timestamp
                };

                // Add each record to a list
                gpxPoints.Add(gpxPoint);
            }
        }
        catch (Exception ex)
        {
            // Handle parsing errors here
            Console.WriteLine("Error parsing GPX file: " + ex.Message);
        }

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

            // Set the url of the GPX point to the Spotify URI
            XmlElement url = document.CreateElement("url");
            url.InnerText = song.spotify_track_uri;
            waypoint.AppendChild(url);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = point.Time.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
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