// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Gpx;

public partial class GPX
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
}
