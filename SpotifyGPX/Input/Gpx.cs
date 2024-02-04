// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public class Gpx
{
    private readonly XDocument document; // Store the document for on-demand reading

    public Gpx(string path)
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

    private bool TracksExist => document.Descendants(Options.InputNs + "trk").Any();

    private bool PointsExist => document.Descendants(Options.InputNs + "trkpt").Any();

    public List<GPXTrack> ParseGpxTracks()
    {
        List<GPXTrack> allTracks = document.Descendants(Options.InputNs + "trk")
            .Select((trk, index) => new GPXTrack( // For each track and its index, create a new GPXTrack
                index,
                trk.Element(Options.InputNs + "name")?.Value,
                TrackType.GPX,
                trk.Descendants(Options.InputNs + "trkpt")
                    .Select((trkpt, pointIndex) => new GPXPoint( // For each point and its index, create a new GPXPoint
                        pointIndex,
                        new Coordinate( // Parse its coordinate
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        trkpt.Element(Options.InputNs + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX")
                    )).ToList() // Send all points to List<GPXPoint>
            ))
            .ToList(); // Send all tracks to List<GPXTrack>

        if (allTracks.Count > 1)
        {
            return HandleMultipleTracks(CombineTracks(CalculateGaps(allTracks)));
        }

        return allTracks;
    }

    private static List<GPXTrack> HandleMultipleTracks(List<GPXTrack> allTracks)
    {
        // Display all the tracks to the user
        DisplayTrackOptions(allTracks);

        int selectedTrackIndex; // Holds the user track selection index

        // Loop the user input request until a valid option is selected
        while (true)
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out selectedTrackIndex) && IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                break; // Return this selection below
            }
            switch (input)
            {
                case "A":
                    return allTracks.Where(track => track.Track.Type == TrackType.GPX).ToList(); // GPX only
                case "B":
                    return allTracks.Where(track => track.Track.Type != TrackType.Combined).ToList(); // GPX and gap tracks
                case "C":
                    return allTracks.Where(track => track.Track.Type == TrackType.Gap).ToList(); // Gaps only
                case "D":
                    return allTracks.Where(track => track.Track.Type != TrackType.Gap).ToList(); // GPX and combined
                case "E":
                    return allTracks.Where(track => track.Track.Type != TrackType.GPX).ToList(); // Gaps and combined
                case "F":
                    return allTracks; // Combined, GPX, and gaps
            }
            Console.WriteLine("Invalid input. Please enter a valid track number.");
        }

        // If the user selected a specific track index, return that
        List<GPXTrack> selectedTracks = new()
        {
            allTracks[selectedTrackIndex]
        };
        return selectedTracks;
    }

    private static void DisplayTrackOptions(List<GPXTrack> allTracks)
    {
        Console.WriteLine("[TRAK] Multiple GPX tracks found:");

        foreach (GPXTrack track in allTracks)
        {
            Console.WriteLine($"[TRAK] Index: {allTracks.IndexOf(track)} {track.ToString()}");
        }

        Console.WriteLine("[TRAK] [A] GPX tracks");
        Console.WriteLine("[TRAK] [B] GPX tracks, and gaps between them");
        Console.WriteLine("[TRAK] [C] Gaps between GPX tracks only");
        Console.WriteLine("[TRAK] [D] GPX tracks and Combined track");
        Console.WriteLine("[TRAK] [E] Gap tracks and Combined track");
        Console.WriteLine("[TRAK] [F] GPX, Gap, and Combined tracks (everything)");
        Console.Write("[TRAK] Please enter the index of the track you want to use: ");
    }

    private static List<GPXTrack> CombineTracks(List<GPXTrack> allTracks)
    {
        // Set up the combined track
        allTracks.Add(new GPXTrack(allTracks.Count, CombinedOrGapTrackName(allTracks.First().Track, allTracks.Last().Track), TrackType.Combined, allTracks.SelectMany(track => track.Points).ToList()));
        return allTracks;
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
                    string gapName = CombinedOrGapTrackName(actualTrack.Track, allTracks[index + 1].Track);

                    if (end.Time != next.Time)
                    {
                        GPXTrack gapTrack = new(index, gapName, TrackType.Gap, new List<GPXPoint> { end, next });

                        return new[] { actualTrack, gapTrack };
                    }
                }

                return new[] { actualTrack };
            })
            .OrderBy(track => track.Track.Index)
            .ToList();
    }

    private static string CombinedOrGapTrackName(TrackInfo track1, TrackInfo track2) => $"{track1.ToString()}-{track2.ToString()}";

    private static bool IsValidTrackIndex(int index, int totalTracks) => index >= 0 && index < totalTracks;
}
