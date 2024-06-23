// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Interfaces with GPS input classes, unifying all formats accepting GPS journeys.
/// </summary>
public partial interface IGpsInput : IDisposable
{
    public StringBroadcaster BCaster { get; }

    /// <summary>
    /// Provides access to a method that parses the <see cref="GpsTrack"/> objects from the file.
    /// </summary>
    public List<GpsTrack> ParseTracksMethod();

    /// <summary>
    /// Gets all GPS tracks as <see cref="GpsTrack"/> objects from this file.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="GpsTrack"/> objects.</returns>
    public List<GpsTrack> GetAllTracks() => ParseTracksMethod();

    /// <summary>
    /// Provides access to a method that parses and filters the <see cref="GpsTrack"/> objects based on file-specific filters.
    /// </summary>
    public List<GpsTrack> FilterTracksMethod();

    /// <summary>
    /// Gets filtered GPS tracks as <see cref="GpsTrack"/> objects, with file-specific filters, from this file.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="GpsTrack"/> objects.</returns>
    List<GpsTrack> GetFilteredTracks() => FilterTracksMethod();

    /// <summary>
    /// Gets tracks based on user-selection.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="GpsTrack"/> objects, based on user-selection</returns>
    public List<GpsTrack> GetSelectedTracks()
    {
        List<GpsTrack> AllTracks = GetAllTracks();

        if (AllTracks.Count > 1)
        {
            // If the GPX contains more than one track, provide user parsing options:

            GpsTrack combinedTrack = GpsTrack.CombineTracks(AllTracks.ToArray()); // Generate a combined track (cohesive of all included tracks)
            AllTracks = CalculateGaps(AllTracks); // Add gaps between tracks as track options
            AllTracks.Add(combinedTrack); // Add the combined track to the end of the list

            return HandleMultipleTracks(AllTracks);
        }

        return AllTracks;
    }

    /// <summary>
    /// Gets input from the user about which tracks to intake.
    /// </summary>
    /// <param name="allTracks">The entire list of tracks.</param>
    /// <returns>A list of GPXTrack objects based on user selection.</returns>
    private List<GpsTrack> HandleMultipleTracks(List<GpsTrack> allTracks)
    {
        int selectedTrackIndex; // Holds the user track selection index        

        BCaster.Broadcast("Multiple GPS tracks found:");

        foreach (GpsTrack track in allTracks)
        {
            BCaster.Broadcast($"Index: {allTracks.IndexOf(track)} {track.ToString()}");
        }

        foreach (var filter in FilterDefinitions)
        {
            BCaster.Broadcast($"[{filter.Key}] {filter.Value}");
        }

        BCaster.Broadcast("Please enter the index of the track you want to use: ");

        // Loop the user input request until a valid option is selected
        while (true)
        {
            string input = Console.ReadLine() ?? string.Empty;

            BCaster.Broadcast($"Read '{input}' as user input from console.", Observation.LogLevel.Debug);

            if (int.TryParse(input, out selectedTrackIndex) && TrackInfo.IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                break; // Return this selection below
            }

            if (MultiTrackFilters.TryGetValue(input, out var FilterFunc))
            {
                return FilterFunc(allTracks).ToList();
            }

            BCaster.BroadcastError(new Exception("Invalid input. Please enter a valid track number."));
        }

        // If the user selected a specific track index, return that
        List<GpsTrack> selectedTracks = new()
        {
            allTracks[selectedTrackIndex]
        };
        return selectedTracks;
    }

    /// <summary>
    /// Calculate all the gaps between tracks.
    /// </summary>
    /// <param name="allTracks">A list of GPXTrack objects.</param>
    /// <returns>A list of GPXTrack objects, containing the original tracks as well as tracks created based on the gaps between each in the original list.</returns>
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
                    string gapName = TrackInfo.CombineTrackNames(gpxTrack.Track, followingTrack.Track); // Create a name for the gap track based on the name of the current track and next track

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

    public bool Disposed { get; }

    /// <summary>
    /// The total number of GPS tracks in the source file.
    /// </summary>
    public int SourceTrackCount { get; }

    /// <summary>
    /// The total number of GPS points in the source file.
    /// </summary>
    public int SourcePointCount { get; }

    /// <summary>
    /// The total number of GPS track objects parsed from the source file to <see cref="GpsTrack"/> objects.
    /// </summary>
    public int ParsedTrackCount { get; }

    /// <summary>
    /// The total number of GPS points parsed from the source file to <see cref="IGpsPoint"/> objects.
    /// </summary>
    public int ParsedPointCount { get; }
}
