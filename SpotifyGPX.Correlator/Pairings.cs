// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SpotifyGPX.Pairings;

public readonly struct Pairings
{
    public Pairings(List<SpotifyEntry> s, List<GPXPoint> p)
    {
        PairedPoints = PairPoints(s, p);
    }

    private readonly List<SongPoint> PairedPoints;

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

        Console.WriteLine($"[GPX] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFile)}' added to GPX");

        return document;
    }
}