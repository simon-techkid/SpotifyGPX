// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Gpx;

public readonly struct GpxFile
{
    private readonly XDocument document;
    private static readonly XNamespace Namespace = "http://www.topografix.com/GPX/1/0";

    public GpxFile(string path)
    {
        try
        {
            // Attempt to load the contents of the specified file into the XML
            document = XDocument.Load(path);
        }
        catch (Exception ex)
        {
            // If the specified XML is invalid, throw an error
            throw new Exception($"The defined {Path.GetExtension(path)} file is incorrectly formatted: {ex.Message}");
        }

        if (!document.Descendants(Namespace + "trk").Any())
        {
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        if (!document.Descendants(Namespace + "trkpt").Any())
        {
            // If there are no <trkpt> point elements in the GPX, throw an error
            throw new Exception($"No points found in '{Path.GetFileName(path)}'!");
        }
    }

    private readonly List<XElement> Tracks
    {
        get
        {
            List<XElement> tracks = document.Descendants(Namespace + "trk").ToList();

            XElement selectedTrack;

            if (tracks.Count > 1)
            {
                // If there are multiple <trk> elements, prompt the user to select one
                Console.WriteLine("[TRAK] Multiple GPX tracks found:");
                for (int i = 0; i < tracks.Count; i++)
                {
                    Console.WriteLine($"[TRAK] [{i + 1}] {tracks[i].Element(Namespace + "name")?.Value ?? $"Track {i}"}");
                }

                Console.WriteLine("[TRAK] [A] All Tracks (Only include songs played during tracks)");
                Console.WriteLine("[TRAK] [B] All Tracks (Include songs played in gaps between tracks)");

                Console.Write("[TRAK] Please choose the track you want to use: ");

                while (true)
                {
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out int selectedTrackIndex) && selectedTrackIndex >= 1 && selectedTrackIndex <= tracks.Count)
                    {
                        // Select the user-chosen <trk> element
                        selectedTrack = tracks[selectedTrackIndex - 1];

                        break;
                    }
                    else if (input == "A")
                    {
                        return tracks;
                    }
                    else if (input == "B")
                    {
                        selectedTrack = new XElement("combined", tracks);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid track number.");
                    }
                }
            }
            else
            {
                selectedTrack = tracks[0];
            }

            List<XElement> finalSelect = new()
            {
                selectedTrack
            };

            return finalSelect;
        }
    }

    public List<GPXPoint> Points
    {
        get
        {
            // Define a dictionary to map <trk> elements to integers.
            Dictionary<XElement, int> trkToIntegerMap = new();

            int trkInteger = 0;

            return Tracks
            .SelectMany(trk =>
            {
                trkToIntegerMap[trk] = trkInteger;

                List<GPXPoint> trkPoints = trk.Descendants(Namespace + "trkpt")
                .Select(trkpt => new GPXPoint
                {
                    Latitude = double.Parse(trkpt.Attribute("lat").Value),
                    Longitude = double.Parse(trkpt.Attribute("lon").Value),
                    TimeStr = trkpt.Element(Namespace + "time").Value,
                    TrackMember = trkToIntegerMap[trk] // Use the retrieved trkInteger
                })
                .ToList();

                trkToIntegerMap[trk] = trkInteger; // Update the dictionary for the next <trk>
                trkInteger++;

                return trkPoints;
            })
            .ToList();
        }
    }
}
