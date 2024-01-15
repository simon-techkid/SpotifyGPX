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
            .Select((trk, index) => new
            {
                TrackElement = trk,
                Index = index,
                Name = trk.Element(Namespace + "name")?.Value,
                Points = trk.Descendants(Namespace + "trkpt").Select((trkpt, pointIndex) =>
                {
                    return new GPXPoint(
                        pointIndex,
                        new Coordinate(
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        trkpt.Element(Namespace + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX")
                    );
                }).ToList()
            })
            .Select(track => new GPXTrack(
                track.Index,
                track.Name,
                track.Points
            ))
            .ToList();

        if (allTracks.Count > 1)
        {
            return HandleMultipleTracks(allTracks);
        }

        return allTracks;
    }

    private static List<GPXTrack> HandleMultipleTracks(List<GPXTrack> allTracks)
    {
        Console.WriteLine("[TRAK] Multiple GPX tracks found:");

        DisplayTrackOptions(allTracks);

        List<GPXTrack> selectedTracks = new();

        while (true)
        {
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int selectedTrackIndex) && IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                selectedTracks.Add(allTracks[selectedTrackIndex]);
                break;
            }
            else if (input == "A") return allTracks;
            else if (input == "B") return CalculateGaps(allTracks, false); // Include both gaps between GPX tracks and original GPX tracks
            else if (input == "C") return MergeTracks(allTracks);
            else if (input == "D") return CalculateGaps(allTracks, true); // Only include gaps between GPX tracks
            else Console.WriteLine("Invalid input. Please enter a valid track number.");
        }

        return selectedTracks;
    }

    private static void DisplayTrackOptions(List<GPXTrack> allTracks)
    {
        allTracks.ForEach(track => Console.WriteLine($"[TRAK] {track}"));
        Console.WriteLine("[TRAK] [A] All tracks");
        Console.WriteLine("[TRAK] [B] All tracks & gap tracks");
        Console.WriteLine("[TRAK] [C] All tracks & gap tracks (flattened)");
        Console.WriteLine("[TRAK] [D] Gap tracks");
        Console.Write("[TRAK] Please enter the index of the track you want to use: ");
    }

    private static List<GPXTrack> MergeTracks(List<GPXTrack> allTracks)
    {
        return new List<GPXTrack>
        {
            new(allTracks.Count, "Flattened", allTracks.SelectMany(track => track.Points).ToList())
        };
    }

    private static List<GPXTrack> CalculateGaps(List<GPXTrack> allTracks, bool onlyGaps)
    {
        List<GPXTrack> gaps = new();

        var resultTracks = allTracks
            .SelectMany((track, index) => // For each track and its index
            {
                if (index < allTracks.Count - 1) 
                {
                    GPXPoint end = track.Points.Last();
                    GPXPoint next = allTracks[index + 1].Points.First();

                    if (end.Time != next.Time)
                    {
                        GPXTrack newTrack = new(index, $"{track.Track}-{allTracks[index + 1].Track}", new List<GPXPoint> { end, next });

                        return onlyGaps ? new[] { newTrack } : new[] { track, newTrack };
                    }
                }

                return onlyGaps ? Array.Empty<GPXTrack>() : new[] { track };
            })
            .ToList();

        return onlyGaps ? resultTracks : resultTracks.OrderBy(track => track.Track.Index).ToList();
    }

    private static bool IsValidTrackIndex(int index, int totalTracks) => (index >= 0) && (index < totalTracks);
}
