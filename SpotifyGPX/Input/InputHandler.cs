using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Handle various input file formats for taking in song and GPS information.
/// </summary>
public class InputHandler
{
    private ISongInput SongInput { get; }
    private IGpsInput GpsInput { get; }
    private IPairInput? PairInput { get; }

    /// <summary>
    /// Creates a handler for taking files as input.
    /// </summary>
    /// <param name="songPath">The path to a file containing Spotify playback records.</param>
    /// <param name="gpsPath">The path to a file containing GPS journey data.</param>
    /// <exception cref="Exception">A provided file does not exist.</exception>
    public InputHandler(string? songPath, string? gpsPath)
    {
        if (!File.Exists(songPath) || songPath == null)
        {
            throw new Exception($"The specified file, '{songPath}', does not exist!");
        }

        if (!File.Exists(gpsPath) || gpsPath == null)
        {
            throw new Exception($"The specified file, '{gpsPath}', does not exist!");
        }

        SongInput = CreateSongInput(songPath);
        GpsInput = CreateGpsInput(gpsPath);

        Console.WriteLine($"[INP] '{Path.GetFileName(gpsPath)}' contains {GpsInput.TrackCount} tracks and {GpsInput.PointCount} points");
        Console.WriteLine($"[INP] '{Path.GetFileName(songPath)}' contains {SongInput.SongCount} total songs");
    }

    /// <summary>
    /// Creates a handler for taking a file as input.
    /// </summary>
    /// <param name="pairPath">The path to a file containing pairing data</param>
    /// <exception cref="Exception">A provided file does not exist.</exception>
    public InputHandler(string? pairPath)
    {
        if (!File.Exists(pairPath) || pairPath == null)
        {
            throw new Exception($"The specified file, '{pairPath}', does not exist!");
        }

        PairInput = CreatePairInput(pairPath);
        SongInput = CreateSongInput(pairPath);
        GpsInput = CreateGpsInput(pairPath);

        Console.WriteLine($"[INP] '{Path.GetFileName(pairPath)}' contains {PairInput.PairCount} total pairs");
    }

    /// <summary>
    /// Gets all song records from the given file.
    /// </summary>
    /// <returns>A list of SpotifyEntries, each representing a single song of playback.</returns>
    public List<SpotifyEntry> GetAllSongs()
    {
        // Returns unfiltered (all) songs
        return SongInput.GetAllSongs();
    }

    /// <summary>
    /// Filters the song records from the given file.
    /// </summary>
    /// <param name="tracks">A list of GPXTracks, by which the contents of the song record list will be filtered.</param>
    /// <returns>A list of SpotifyEntries, each representing a single song of playback.</returns>
    public List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks)
    {
        // Returns filtered songs
        return SongInput.GetFilteredSongs(tracks);
    }

    /// <summary>
    /// Gets all journey tracks from the given file.
    /// </summary>
    /// <returns>A list of GPXTracks, each representing a collection of positions comprising a journey's path.</returns>
    public List<GPXTrack> GetAllTracks()
    {
        // Return all tracks
        return GpsInput.GetAllTracks();
    }

    /// <summary>
    /// Gets user-selected journey track(s) from the given file.
    /// </summary>
    /// <returns>A list of GPXTrack(s) based on user selection, each representing a collection of positions comprising a journey's path</returns>
    public List<GPXTrack> GetSelectedTracks()
    {
        return GpsInput.GetSelectedTracks();
    }

    /// <summary>
    /// Gets all Song-Point pairs from the given file.
    /// </summary>
    /// <returns>A list of SongPoint objects, each representing an already-paired Song and Point.</returns>
    public List<SongPoint> GetAllPairs()
    {
        if (PairInput != null)
        {
            return PairInput.GetAllPairs();
        }
        else
        {
            throw new Exception($"Unable to get pairs: this input format does not support pairs.");
        }
    }

    /// <summary>
    /// Determines the appropriate import class for handling this song records file.
    /// </summary>
    /// <param name="path">The path to the song records file.</param>
    /// <returns>An ISongInput interface allowing interfacing with the corresponding format.</returns>
    /// <exception cref="Exception">The provided file doesn't have an import class associated with it.</exception>
    private static ISongInput CreateSongInput(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        return extension switch
        {
            ".json" => new Json(path),
            ".jsonreport" => new JsonReport(path),
            _ => throw new Exception($"Unsupported song file format: {extension}"),
        };
    }

    /// <summary>
    /// Determines the appropriate import class for handling this GPS journey file.
    /// </summary>
    /// <param name="path">The path to the GPS journey file.</param>
    /// <returns>An IGpsInput interface allowing interfacing with the corresponding format.</returns>
    /// <exception cref="Exception">The provided file doesn't have an import class associated with it.</exception>
    private static IGpsInput CreateGpsInput(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        return extension switch
        {
            ".gpx" => new Gpx(path),
            ".jsonreport" => new JsonReport(path),
            _ => throw new Exception($"Unsupported GPS file format: {extension}"),
        };
    }

    private static IPairInput CreatePairInput(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        return extension switch
        {
            ".jsonreport" => new JsonReport(path),
            _ => throw new Exception($"Unsupported pairs file format: {extension}")
        };
    }
}

/// <summary>
/// A list of the accepted formats containing song records.
/// </summary>
public enum SongFormats
{
    /// <summary>
    /// A JSON file containing user playback data in the Spotify format.
    /// </summary>
    Json,

    /// <summary>
    /// A .jsonreport file created by SpotifyGPX that can be used as input.
    /// </summary>
    JsonReport
}

/// <summary>
/// A list of the accepted formats containing GPS journeys.
/// </summary>
public enum GpsFormats
{
    /// <summary>
    /// A GPX file containing geospatial information for a journey.
    /// </summary>
    Gpx,

    /// <summary>
    /// A .jsonreport file created by SpotifyGPX that can be used as input.
    /// </summary>
    JsonReport
}

public interface IPairInput
{
    public List<SongPoint> GetAllPairs();
    int PairCount { get; }
}

/// <summary>
/// Interfaces with song input classes, unifying all formats accepting song records.
/// </summary>
public interface ISongInput
{
    private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time (0,0,0 for all songs)
    private static bool ExcludeSkipped => false; // Ignore songs skipped by the user, as defined by Spotify JSON (false for all songs)

    /// <summary>
    /// Gets all songs in the file.
    /// </summary>
    /// <returns>A list of SpotifyEntry objects.</returns>
    List<SpotifyEntry> GetAllSongs();

    /// <summary>
    /// Get songs based on an existing list of track times.
    /// </summary>
    /// <param name="tracks">A list of GPXTrack objects.</param>
    /// <returns>A list of SpotifyEntry objects that must be within the times of the GPXTrack object(s).</returns>
    List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks)
    {
        List<SpotifyEntry> AllSongs = GetAllSongs();

        var trackRange = tracks.Select(track => (track.Start, track.End)).ToList();

        // FilterEntries() differs from AllSongs because it filters the entire JSON file by the following parameters:
        // The song must have been played during the GPS tracking timeframe (but PairingsHandler.PairPoints() filters this too)
        // The song must have been played for longer than the MinimumPlaytime TimeSpan (beginning of this file)
        // The song must have not been skipped during playback by the user (if ExcludeSkipped is true)

        // You may add other filtration options below, within the .Any() statement:

        List<SpotifyEntry> FilteredSongs = AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes =>
                spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End) && // Within the time range of tracks
                (spotifyEntry.TimePlayed == null || spotifyEntry.TimePlayed >= MinimumPlaytime) && // Long enough duration
                (spotifyEntry.Song_Skipped != true || !ExcludeSkipped))) // Wasn't skipped
            .ToList(); // Send the songs passing the filter to a list

        Console.WriteLine($"[INP] {FilteredSongs.Count} songs filtered from {AllSongs.Count} total");

        return FilteredSongs;
    }

    /// <summary>
    /// The total number of songs in the given file.
    /// </summary>
    int SongCount { get; }
}

/// <summary>
/// Interfaces with GPS input classes, unifying all formats accepting GPS journeys.
/// </summary>
public interface IGpsInput
{
    /// <summary>
    /// Gets all tracks in the file.
    /// </summary>
    /// <returns>A list of GPXTrack objects.</returns>
    List<GPXTrack> GetAllTracks();

    /// <summary>
    /// Gets tracks based on user-selection.
    /// </summary>
    /// <returns>A list of GPXTrack objects, based on user-selection</returns>
    List<GPXTrack> GetSelectedTracks()
    {
        List<GPXTrack> AllTracks = GetAllTracks();

        if (AllTracks.Count > 1)
        {
            // If the GPX contains more than one track, provide user parsing options:

            GPXTrack combinedTrack = CombineTracks(AllTracks); // Generate a combined track (cohesive of all included tracks)
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
    private static List<GPXTrack> HandleMultipleTracks(List<GPXTrack> allTracks)
    {
        // Display all the tracks to the user
        DisplayTrackOptions(allTracks);

        int selectedTrackIndex; // Holds the user track selection index

        // Loop the user input request until a valid option is selected
        while (true)
        {
            string input = Console.ReadLine() ?? string.Empty;
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

    /// <summary>
    /// Write each track to the console, and display user options for intake selection.
    /// </summary>
    /// <param name="allTracks">The entire list of tracks.</param>
    private static void DisplayTrackOptions(List<GPXTrack> allTracks)
    {
        Console.WriteLine("[INP] Multiple GPX tracks found:");

        foreach (GPXTrack track in allTracks)
        {
            Console.WriteLine($"[INP] Index: {allTracks.IndexOf(track)} {track.ToString()}");
        }

        Console.WriteLine("[INP] [A] GPX tracks");
        Console.WriteLine("[INP] [B] GPX tracks, and gaps between them");
        Console.WriteLine("[INP] [C] Gaps between GPX tracks only");
        Console.WriteLine("[INP] [D] GPX tracks and Combined track");
        Console.WriteLine("[INP] [E] Gap tracks and Combined track");
        Console.WriteLine("[INP] [F] GPX, Gap, and Combined tracks (everything)");
        Console.Write("[INP] Please enter the index of the track you want to use: ");
    }

    /// <summary>
    /// Combine a list of tracks into a single track.
    /// </summary>
    /// <param name="allTracks">A list of GPXTrack objects.</param>
    /// <returns>A single GPXTrack with data from each in the list.</returns>
    /// <exception cref="Exception">The list provided was null or contained no tracks.</exception>
    private static GPXTrack CombineTracks(List<GPXTrack> allTracks)
    {
        if (allTracks == null || allTracks.Count == 0)
        {
            throw new Exception("No tracks provided to combine!");
        }

        // Combine all points from all tracks
        var combinedPoints = allTracks.SelectMany(track => track.Points);

        // Create a new GPXTrack with combined points
        GPXTrack combinedTrack = new(allTracks.Count, CombinedOrGapTrackName(allTracks.First().Track, allTracks.Last().Track), TrackType.Combined, combinedPoints.ToList());

        return combinedTrack;
    }

    /// <summary>
    /// Calculate all the gaps between tracks.
    /// </summary>
    /// <param name="allTracks">A list of GPXTrack objects.</param>
    /// <returns>A list of GPXTrack objects, containing the original tracks as well as tracks created based on the gaps between each in the original list.</returns>
    private static List<GPXTrack> CalculateGaps(List<GPXTrack> allTracks)
    {
        return allTracks
            .SelectMany((gpxTrack, index) => // For each track and its index
            {
                if (index < allTracks.Count - 1)
                {
                    GPXTrack followingTrack = allTracks[index + 1]; // Get the track after the current track (next one)
                    GPXPoint end = gpxTrack.Points.Last(); // Get the last point of the current track
                    GPXPoint next = followingTrack.Points.First(); // Get the first point of the next track
                    string gapName = CombinedOrGapTrackName(gpxTrack.Track, followingTrack.Track); // Create a name for the gap track based on the name of the current track and next track

                    if (end.Time != next.Time)
                    {
                        // Create a gap track based on the index of this track, the name of the gap, and the two endpoints                        
                        GPXTrack gapTrack = new(index, gapName, TrackType.Gap, new List<GPXPoint> { end, next });

                        // Return this track, and the gap between it and the next track
                        return new[] { gpxTrack, gapTrack };
                    }
                }

                return new[] { gpxTrack }; // If there's no gap, return the GPX track
            })
            .OrderBy(track => track.Track.Index) // Order all tracks by index
            .ToList();
    }

    /// <summary>
    /// Creates a friendly name for a bridge track (combination or gap track) between GPXTrack objects.
    /// </summary>
    /// <param name="track1">The track before the break.</param>
    /// <param name="track2">The track after the break.</param>
    /// <returns>A name combining the names of the two given tracks.</returns>
    private static string CombinedOrGapTrackName(TrackInfo track1, TrackInfo track2) => $"{track1.ToString()}-{track2.ToString()}";

    /// <summary>
    /// Determines whether a user-input track selection is valid.
    /// </summary>
    /// <param name="index">The user-provided index of a GPXTrack.</param>
    /// <param name="totalTracks">The total number of tracks available for selection.</param>
    /// <returns>True, if the user-provided index is an existing GPXTrack.</returns>
    private static bool IsValidTrackIndex(int index, int totalTracks) => index >= 0 && index < totalTracks;

    /// <summary>
    /// The total number of tracks in the given file.
    /// </summary>
    int TrackCount { get; }

    /// <summary>
    /// The total number of points in the given file.
    /// </summary>
    int PointCount { get; }
}
