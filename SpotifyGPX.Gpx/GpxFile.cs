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
    private readonly XDocument document; // Store the document for on-demand reading
    private static readonly XNamespace Namespace = "http://www.topografix.com/GPX/1/0"; // Default namespace

    public GpxFile(string path)
    {
        document = LoadDocument(path);

        if (!TracksExist)
        {
            // If there are no <trk> tracks in the GPX, throw error
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        if (!PointsExist)
        {
            // If there are no <trkpt> points the GPX, throw error
            throw new Exception($"No points found in '{Path.GetFileName(path)}'!");
        }
    }

    private static XDocument LoadDocument(string path)
    {
        try
        {
            return XDocument.Load(path);
        }
        catch (Exception ex)
        {
            throw new Exception($"The GPX file is incorrectly formatted: {ex.Message}");
        }
    }

    private readonly bool TracksExist => document.Descendants(Namespace + "trk").Any();

    private readonly bool PointsExist => document.Descendants(Namespace + "trkpt").Any();

    private List<XElement> GetTracks()
    {
        // List all the tracks in the document
        List<XElement> allTracks = document.Descendants(Namespace + "trk").ToList();

        // Handle multiple tracks if there are more than one
        if (allTracks.Count > 1)
        {
            // If there are multiple tracks, ask the user which track to use
            return HandleMultipleTracks(allTracks);
        }

        // If there is one track, return the original list
        return allTracks;
    }

    private static List<XElement> HandleMultipleTracks(List<XElement> allTracks)
    {
        // If there are multiple <trk> elements, prompt the user to select one
        Console.WriteLine("[TRAK] Multiple GPX tracks found:");
        for (int i = 0; i < allTracks.Count; i++)
        {
            // Print out all the tracks' name values, if they have a name) or an ascending numeric name
            Console.WriteLine($"[TRAK] [{i}] {allTracks[i].Element(Namespace + "name")?.Value ?? $"Track {i}"}");
        }

        Console.WriteLine("[TRAK] [A] All Tracks (Only include songs played during GPX tracking)");
        Console.WriteLine("[TRAK] [B] All Tracks (Include songs played both during & between GPX tracks)");

        Console.Write("[TRAK] Please choose the track you want to use: ");

        // Create a list for the user selected track(s)
        List<XElement> selectedTracks = new();

        // Forever, until the user provides valid input:
        while (true)
        {
            string? input = Console.ReadLine(); // Read user input

            // Pair only songs included in the track the user selects:
            if (int.TryParse(input, out int selectedTrackIndex) && selectedTrackIndex >= 0 && selectedTrackIndex <= allTracks.Count)
            {
                selectedTracks.Add(allTracks[selectedTrackIndex]); // Select the user-chosen <trk> element
                break; // Exit the prompt loop
            }
            // Pair all songs in the GPX regardless of track:
            else if (input == "A")
            {
                // Return the original list of distinguished tracks
                return allTracks;
            }
            // Pair all songs in the GPX regardless or track, and regardless of whether they were listened to during GPX journey:
            else if (input == "B")
            {
                // Aggregate all the tracks of the GPX into a single track (they will be cohesive):
                return new List<XElement> { new("combined", allTracks) };
            }
            // User did not provide valid input:
            else
            {
                // Go back to the beginning of the prompt and ask the user again
                Console.WriteLine("Invalid input. Please enter a valid track number.");
            }
        }

        return selectedTracks;
    }

    public readonly List<GPXTrack> ParseGpxTracks()
    {
        return GetTracks()
        .SelectMany(trk => trk.Descendants(Namespace + "trkpt")
            .Select(trkpt => new
            {
                TrackElement = trk,
                Coordinate = new Coordinate(
                            trkpt.Attribute("lat")?.Value ?? throw new Exception("GPX point 'lat' cannot be null, check your GPX"),
                            trkpt.Attribute("lon")?.Value ?? throw new Exception("GPX point 'lon' cannot be null, check your GPX")
                        ),
                Time = trkpt.Element(Namespace + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX"),
            }))
            .GroupBy(data => data.TrackElement)
                .Select((track, index) =>
                {
                    List<GPXPoint> trkPoints = track
                        .Select((pointData, pointIndex) => new GPXPoint(
                            pointData.Coordinate,
                            pointData.Time,
                            pointIndex
                        ))
                        .ToList();

                    return new GPXTrack(trkPoints, index);
                })
                .ToList();
    }

}
