// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SpotifyGPX.Gpx;

public partial class GPX
{
    public static XmlDocument CreateGPXFile(List<SongPoint> finalPoints, string gpxFile, string desc)
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

        foreach (SongPoint pair in finalPoints)
        {
            // Create waypoint for each song
            XmlElement waypoint = document.CreateElement("wpt");
            GPX.AppendChild(waypoint);

            // Set the lat and lon of the waypoing to the original point
            waypoint.SetAttribute("lat", pair.Point.Latitude.ToString());
            waypoint.SetAttribute("lon", pair.Point.Longitude.ToString());

            // Set the name of the GPX point to the name of the song
            XmlElement name = document.CreateElement("name");
            name.InnerText = Point.GpxTitle(pair.Song);
            waypoint.AppendChild(name);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = pair.Point.Time.ToUniversalTime().ToString(Point.gpxTimeOut);
            waypoint.AppendChild(time);

            // Set the description of the point 
            XmlElement description = document.CreateElement("desc");
            description.InnerText = Point.GpxDescription(pair);
            waypoint.AppendChild(description);

            pointCount++;
        }

        Console.WriteLine($"[GPX] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFile)}' added to GPX");

        return document;
    }
}
