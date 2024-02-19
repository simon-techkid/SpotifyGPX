using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public class InputHandler
{
    private ISongInput SongInput { get; }
    private IGpsInput GpsInput { get; }

    /// <summary>
    /// Creates a handler for taking files as input.
    /// </summary>
    /// <param name="songPath">The path to a file containing Spotify playback records.</param>
    /// <param name="gpsPath">The path to a file containing GPS journey data.</param>
    /// <exception cref="Exception">A provided file does not exist.</exception>
    public InputHandler(string songPath, string gpsPath)
    {
        if (!File.Exists(songPath))
        {
            throw new Exception($"The specified file, '{songPath}', does not exist!");
        }

        if (!File.Exists(gpsPath))
        {
            throw new Exception($"The specified file, '{gpsPath}', does not exist!");
        }

        SongInput = CreateSongInput(songPath);
        GpsInput = CreateGpsInput(gpsPath);

        Console.WriteLine($"[INP] '{Path.GetFileName(gpsPath)}' contains {GpsInput.TrackCount} tracks and {GpsInput.PointCount} points");
        Console.WriteLine($"[INP] '{Path.GetFileName(songPath)}' contains {SongInput.Count} total songs");
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

/// <summary>
/// Interfaces with song input classes, unifying all formats accepting song records.
/// </summary>
public interface ISongInput
{
    private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time (0,0,0 for all songs)
    private static bool ExcludeSkipped => false; // Ignore songs skipped by the user, as defined by Spotify JSON (false for all songs)

    List<SpotifyEntry> GetAllSongs();

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

    int Count { get; }
}

/// <summary>
/// Interfaces with GPS input classes, unifying all formats accepting GPS journeys.
/// </summary>
public interface IGpsInput
{
    List<GPXTrack> GetAllTracks();
    int TrackCount { get; }
    int PointCount { get; }
}
