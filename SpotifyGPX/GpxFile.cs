// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX;

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

    public List<GPXTrack> ParseGpxTracks()
    {
        List<GPXTrack> allTracks = document.Descendants(Namespace + "trk")
            .Select((trk, index) => new GPXTrack( // For each track and its index, create a new GPXTrack
                index,
                trk.Element(Namespace + "name")?.Value,
                false,
                trk.Descendants(Namespace + "trkpt")
                    .Select((trkpt, pointIndex) => new GPXPoint( // For each point and its index, create a new GPXPoint
                        pointIndex,
                        new Coordinate( // Parse its coordinate
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        trkpt.Element(Namespace + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX")
                    )).ToList() // Send all points to List<GPXPoint>
            ))
            .ToList(); // Send all tracks to List<GPXTrack>

        if (allTracks.Count > 1)
        {
            return HandleMultipleTracks(CalculateGaps(allTracks));
        }

        return allTracks;
    }

    private static List<GPXTrack> HandleMultipleTracks(List<GPXTrack> allTracks)
    {
        Console.WriteLine("[TRAK] Multiple GPX tracks found:");

        DisplayTrackOptions(allTracks);

        List<GPXTrack> selectedTracks = new();

        int selectedTrackIndex;

        while (true)
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out selectedTrackIndex) && IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                break;
            }
            switch (input)
            {
                case "A":
                    return allTracks.Where(track => track.Track.Gaps == false).ToList();
                case "B":
                    return allTracks;
                case "C":
                    return allTracks.Where(track => track.Track.Gaps == true).ToList();
            }
            Console.WriteLine("Invalid input. Please enter a valid track number.");
        }

        selectedTracks.Add(allTracks[selectedTrackIndex]);
        return selectedTracks;
    }

    private static void DisplayTrackOptions(List<GPXTrack> allTracks)
    {
        foreach (GPXTrack track in allTracks)
        {
            Console.WriteLine($"[TRAK] [{allTracks.IndexOf(track)}] {track}");
        }

        Console.WriteLine("[TRAK] [A] All tracks");
        Console.WriteLine("[TRAK] [B] All tracks & gap tracks");
        Console.WriteLine("[TRAK] [C] Gap tracks");
        Console.Write("[TRAK] Please enter the index of the track you want to use: ");
    }

    private static List<GPXTrack> CalculateGaps(List<GPXTrack> allTracks)
    {
        return allTracks
            .SelectMany((actualTrack, index) => // For each track and its index
            {
                if (index < allTracks.Count - 1)
                {
                    GPXPoint end = actualTrack.Points.Last();
                    GPXPoint next = allTracks[index + 1].Points.First();
                    string gapName = $"{actualTrack.Track}-{allTracks[index + 1].Track}";

                    if (end.Time != next.Time)
                    {
                        GPXTrack gapTrack = new(index, gapName, true, new List<GPXPoint> { end, next });

                        return new[] { actualTrack, gapTrack };
                    }
                }

                return new[] { actualTrack };
            })
            .OrderBy(track => track.Track.Index)
            .ToList();
    }

    private static bool IsValidTrackIndex(int index, int totalTracks) => (index >= 0) && (index < totalTracks);
}
