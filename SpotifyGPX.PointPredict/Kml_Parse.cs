// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SpotifyGPX.Options;

namespace SpotifyGPX.PointPredict;

partial class PointPredict
{
    private static List<(double, double)> ParseKmlFile(string kmlFile)
    {
        // Create a new XML document
        XmlDocument doc = new();

        // Use the GPX namespace
        XmlNamespaceManager nsManager = new(doc.NameTable);
        nsManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

        // Create a list of intermediate coordinates
        List<(double, double)> coordinates = new();

        try
        {
            // Attempt to load the contents of the specified file into the XML
            doc.Load(kmlFile);
        }
        catch (Exception ex)
        {
            // If the specified XML is invalid, throw an error
            throw new Exception($"The defined {Path.GetExtension(kmlFile)} file is incorrectly formatted: {ex.Message}");
        }

        try
        {
            // Select all LineString coordinates
            XmlNodeList coordinatesNodes = doc.SelectNodes("//kml:LineString/kml:coordinates", nsManager);

            foreach (XmlNode coordinatesNode in coordinatesNodes)
            {
                string[] coordinateStrings = coordinatesNode.InnerText.Trim().Split(' ');

                foreach (string coordinateString in coordinateStrings)
                {
                    string[] parts = coordinateString.Split(',');

                    if (parts.Length >= 2 && double.TryParse(parts[0], out double longitude) && double.TryParse(parts[1], out double latitude))
                    {
                        coordinates.Add((latitude, longitude));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing {Path.GetExtension(kmlFile)} file: {ex.Message}");
        }

        List<GPXPoint> kmlPoints = coordinates
        .Select(point => new GPXPoint
        {
            Latitude = point.Item1,
            Longitude = point.Item2
        })
        .ToList();

        return coordinates;
    }
}
