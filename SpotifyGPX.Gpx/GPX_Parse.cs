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
    private static XDocument document = new();
    private static List<GPXPoint> Points { get; set; }
    private static readonly XNamespace Namespace = "http://www.topografix.com/GPX/1/0";

    public static List<GPXPoint> ParseGPXFile(string gpxFile)
    {
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

        if (!document.Descendants(Namespace + "trk").Any())
        {
            throw new Exception($"No track elements found in '{Path.GetFileName(gpxFile)}'!");
        }

        if (!document.Descendants(Namespace + "trkpt").Any())
        {
            // If there are no <trkpt> point elements in the GPX, throw an error
            throw new Exception($"No points found in '{Path.GetFileName(gpxFile)}'!");
        }
                
        XElement selectedTrack = TrackManager();

        ParseTrack(selectedTrack);
        
        // Return the list of points from the GPX
        return Points;
    }

    private static void ParseTrack(XElement track)
    {
        // Attempt to add all GPX <trkpt> latitudes, longitudes, and times to the gpxPoints list
        Points = track.Descendants(Namespace + "trkpt")
        .Select(trkpt => new GPXPoint
        {
            Latitude = double.Parse(trkpt.Attribute("lat").Value),
            Longitude = double.Parse(trkpt.Attribute("lon").Value),
            Time = DateTimeOffset.ParseExact(trkpt.Element(Namespace + "time").Value, Point.gpxPointTimeInp, null)
        })
        .ToList();
    }
}
