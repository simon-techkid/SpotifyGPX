// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Gpx;

public partial class Parser
{
    public static List<GPXPoint> ParseGPXFile(string gpxFile)
    {
        // Create a new XML document
        XDocument document = new();

        // Use the GPX namespace
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
            throw new Exception($"The defined {Path.GetExtension(gpxFile)} file is incorrectly formatted: {ex.Message}");
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
                Latitude = double.Parse(trkpt.Attribute("lat").Value),
                Longitude = double.Parse(trkpt.Attribute("lon").Value),
                Time = DateTimeOffset.ParseExact(trkpt.Element(ns + "time").Value, Point.gpxPointTimeInp, null)
            })
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"The GPX parameter cannot be parsed:\n{ex.Message}");
        }

        // Return the list of points from the GPX
        return gpxPoints;
    }

    public static XmlDocument CreateGPXFile(List<(SpotifyEntry, GPXPoint, int)> finalPoints, string gpxFile)
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

        foreach ((SpotifyEntry song, GPXPoint point, _) in finalPoints)
        {
            // Create waypoint for each song
            XmlElement waypoint = document.CreateElement("wpt");
            GPX.AppendChild(waypoint);

            // Set the lat and lon of the waypoing to the original point
            waypoint.SetAttribute("lat", point.Latitude.ToString());
            waypoint.SetAttribute("lon", point.Longitude.ToString());

            // Set the name of the GPX point to the name of the song
            XmlElement name = document.CreateElement("name");
            name.InnerText = Point.GpxTitle(song);
            waypoint.AppendChild(name);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = point.Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            waypoint.AppendChild(time);

            // Set the description of the point 
            XmlElement description = document.CreateElement("desc");
            description.InnerText = Point.GpxDescription(song, point.Time.Offset, point.Predicted == true ? "Point Predicted" : null);
            waypoint.AppendChild(description);
            pointCount++;
        }

        Console.WriteLine($"[GPX] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFile)}' added to GPX");

        return document;
    }
}
