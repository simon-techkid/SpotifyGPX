// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SpotifyGPX.Pairings;

public readonly struct Pairings
{
    public Pairings(List<SpotifyEntry> s, List<GPXPoint> p) => PairedPoints = PairPoints(s, p);

    public Pairings(Pairings organic, string? kmlFile) => PairedPoints = new Prediction(organic.PairedPoints, kmlFile).Predicted;
    
    private readonly List<SongPoint> PairedPoints;

    private readonly List<SpotifyEntry> Songs => PairedPoints.Select(pair => pair.Song).ToList();

    private readonly List<GPXPoint> Points => PairedPoints.Select(pair => pair.Point).ToList();

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXPoint> points)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<SongPoint> correlatedEntries = songs
        .Select((spotifyEntry, index) =>
        {
            GPXPoint nearestPoint = points
            .OrderBy(point => Math.Abs((point.Time - spotifyEntry.Time).TotalSeconds))
            .First();

            SongPoint pair = new(spotifyEntry, nearestPoint, index);
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

    public readonly bool CreateGPX(string path, string desc)
    {
        File.Delete(path);
        
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
        gpxName.InnerText = Path.GetFileNameWithoutExtension(path);
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
            waypoint.SetAttribute("lat", pair.Point.Location.Latitude.ToString());
            waypoint.SetAttribute("lon", pair.Point.Location.Longitude.ToString());

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

        Console.WriteLine($"[GPX] {pointCount} points found in '{Path.GetFileNameWithoutExtension(path)}' added to GPX");

        document.Save(path);
        return File.Exists(path);
    }

    public readonly bool JsonToFile(string path)
    {
        File.Delete(path);
        
        // Create a list of JSON objects
        List<JObject> json = new();

        foreach (SpotifyEntry song in Songs)
        {
            // Attempt to parse each SpotifyEntry to a JSON object
            try
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["ts"] = song.TimeStr,
                    ["username"] = song.Spotify_Username,
                    ["platform"] = song.Spotify_Platform,
                    ["ms_played"] = song.Time_Played,
                    ["conn_country"] = song.Spotify_Country,
                    ["ip_addr_decrypted"] = song.Spotify_IP,
                    ["user_agent_decrypted"] = song.Spotify_UA,
                    ["master_metadata_track_name"] = song.Song_Name,
                    ["master_metadata_album_artist_name"] = song.Song_Artist,
                    ["master_metadata_album_album_name"] = song.Song_Album,
                    ["spotify_track_uri"] = song.Song_URI,
                    ["episode_name"] = song.Episode_Name,
                    ["episode_show_name"] = song.Episode_Show,
                    ["spotify_episode_uri"] = song.Episode_URI,
                    ["reason_start"] = song.Song_StartReason,
                    ["reason_end"] = song.Song_EndReason,
                    ["shuffle"] = song.Song_Shuffle,
                    ["skipped"] = song.Song_Skipped,
                    ["offline"] = song.Spotify_Offline,
                    ["offline_timestamp"] = song.Spotify_OfflineTS,
                    ["incognito"] = song.Spotify_Incognito
                };

                // Add the SpotifyEntry JObject to the list
                json.Add(songEntry);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending track, '{song.Song_Name}', to JSON: {ex.Message}");
            }
        }

        // Create a JSON document based on the list of songs within range
        string document = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);

        File.WriteAllText(path, document);
        return File.Exists(path);
    }

    public readonly bool JsonUriToFile(string path)
    {
        File.Delete(path);
        
        // Create string for final clipboard contents
        string clipboard = "";

        foreach (SpotifyEntry song in Songs)
        {
            // Ensures no null values return
            if (song.Song_URI != null)
            {
                clipboard += $"{song.Song_URI}\n";
            }
            else
            {
                // If null URI, throw exception
                throw new Exception($"URI null for track '{song.Song_Name}'");
            }
        }

        File.WriteAllText(path, clipboard);
        return File.Exists(path);
    }

    public readonly bool PlaylistToFile(string path)
    {
        File.Delete(path);
        
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
        name.InnerText = Path.GetFileNameWithoutExtension(path);
        XSPF.AppendChild(name);

        // Set the title of the XSPF playlist to the name of the file
        XmlElement creator = document.CreateElement("creator");
        creator.InnerText = "SpotifyGPX";
        XSPF.AppendChild(creator);

        // Create the trackList header
        XmlElement trackList = document.CreateElement("trackList");
        XSPF.AppendChild(trackList);

        foreach (SpotifyEntry song in Songs)
        {
            // Create track for each song
            XmlElement track = document.CreateElement("track");
            trackList.AppendChild(track);

            // Set the creator of the track to the song artist
            XmlElement artist = document.CreateElement("creator");
            artist.InnerText = song.Tag(SpotifyEntry.ReturnTag.Creator);
            track.AppendChild(artist);

            // Set the title of the track to the song name
            XmlElement title = document.CreateElement("title");
            title.InnerText = song.Tag(SpotifyEntry.ReturnTag.Title);
            track.AppendChild(title);

            // Set the annotation of the song to the end time
            XmlElement annotation = document.CreateElement("annotation");
            annotation.InnerText = song.Tag(SpotifyEntry.ReturnTag.Annotation);
            track.AppendChild(annotation);

            // Set the duration of the song to the amount of time it was listened to
            XmlElement duration = document.CreateElement("duration");
            duration.InnerText = song.Tag(SpotifyEntry.ReturnTag.Duration);
            track.AppendChild(duration);
        }

        document.Save(path);
        return File.Exists(path);
    }
}