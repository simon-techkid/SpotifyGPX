// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with GPS input classes, unifying all formats accepting GPS journeys.
/// </summary>
public partial interface IGpsInput : IFileInput
{
    /// <summary>
    /// Gets all tracks in the file.
    /// </summary>
    /// <returns>A list of <see cref="GpsTrack"/> objects.</returns>
    List<GpsTrack> GetAllTracks();

    /// <summary>
    /// Gets filtered tracks based file-specific filters.
    /// </summary>
    /// <returns>A list of <see cref="GpsTrack"/> objects.</returns>
    List<GpsTrack> GetFilteredTracks();

    /// <summary>
    /// Gets tracks based on user-selection.
    /// </summary>
    /// <returns>A list of <see cref="GpsTrack"/> objects, based on user selection parameters.</returns>
    List<GpsTrack> GetSelectedTracks()
    {
        List<GpsTrack> AllTracks = GetAllTracks();

        if (AllTracks.Count > 1)
        {
            // If the GPX contains more than one track, provide user parsing options:

            GpsTrack combinedTrack = CombineTracks(AllTracks); // Generate a combined track (cohesive of all included tracks)
            AllTracks = CalculateGaps(AllTracks); // Add gaps between tracks as track options
            AllTracks.Add(combinedTrack); // Add the combined track to the end of the list

            return HandleMultipleTracks(AllTracks);
        }

        return AllTracks;
    }

    private static List<GpsTrack> HandleMultipleTracks(List<GpsTrack> allTracks)
    {
        int selectedTrackIndex; // Holds the user track selection index        

        Console.WriteLine("[INP] Multiple GPX tracks found:");

        foreach (GpsTrack track in allTracks)
        {
            Console.WriteLine($"[INP] Index: {allTracks.IndexOf(track)} {track.ToString()}");
        }

        foreach (var filter in FilterDefinitions)
        {
            Console.WriteLine($"[INP] [{filter.Key}] {filter.Value}");
        }

        Console.Write("[INP] Please enter the index of the track you want to use: ");

        // Loop the user input request until a valid option is selected
        while (true)
        {
            string input = Console.ReadLine() ?? string.Empty;
            if (int.TryParse(input, out selectedTrackIndex) && IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                break; // Return this selection below
            }

            if (MultiTrackFilters.TryGetValue(input, out var FilterFunc))
            {
                return FilterFunc(allTracks).ToList();
            }

            Console.WriteLine("Invalid input. Please enter a valid track number.");
        }

        // If the user selected a specific track index, return that
        List<GpsTrack> selectedTracks = new()
        {
            allTracks[selectedTrackIndex]
        };
        return selectedTracks;
    }

    private static GpsTrack CombineTracks(List<GpsTrack> allTracks)
    {
        if (allTracks == null || allTracks.Count == 0)
        {
            throw new Exception("No tracks provided to combine!");
        }

        // Combine all points from all tracks
        var combinedPoints = allTracks.SelectMany(track => track.Points);

        // Create a new GPXTrack with combined points
        GpsTrack combinedTrack = new(allTracks.Count, CombinedOrGapTrackName(allTracks.First().Track, allTracks.Last().Track), TrackType.Combined, combinedPoints.ToList());

        return combinedTrack;
    }

    private static List<GpsTrack> CalculateGaps(List<GpsTrack> allTracks)
    {
        return allTracks
            .SelectMany((gpxTrack, index) => // For each track and its index
            {
                if (index < allTracks.Count - 1)
                {
                    GpsTrack followingTrack = allTracks[index + 1]; // Get the track after the current track (next one)
                    IGpsPoint end = gpxTrack.Points.Last(); // Get the last point of the current track
                    IGpsPoint next = followingTrack.Points.First(); // Get the first point of the next track
                    string gapName = CombinedOrGapTrackName(gpxTrack.Track, followingTrack.Track); // Create a name for the gap track based on the name of the current track and next track

                    if (end.Time != next.Time)
                    {
                        // Create a gap track based on the index of this track, the name of the gap, and the two endpoints                        
                        GpsTrack gapTrack = new(index, gapName, TrackType.Gap, new List<IGpsPoint> { end, next });

                        // Return this track, and the gap between it and the next track
                        return new[] { gpxTrack, gapTrack };
                    }
                }

                return new[] { gpxTrack }; // If there's no gap, return the GPX track
            })
            .OrderBy(track => track.Track.Index) // Order all tracks by index
            .ToList();
    }

    private static string CombinedOrGapTrackName(TrackInfo track1, TrackInfo track2) => $"{track1.ToString()}-{track2.ToString()}";

    private static bool IsValidTrackIndex(int index, int totalTracks) => index >= 0 && index < totalTracks;

    /// <summary>
    /// The total number of GPS tracks in the source file.
    /// </summary>
    int SourceTrackCount { get; }

    /// <summary>
    /// The total number of GPS points in the source file.
    /// </summary>
    int SourcePointCount { get; }

    /// <summary>
    /// The total number of GPS tracks parsed from the source file to GPXTrack objects.
    /// </summary>
    int ParsedTrackCount { get; }

    /// <summary>
    /// The total number of GPS points parsed from the source file to GPXPoint objects.
    /// </summary>
    int ParsedPointCount { get; }
}
