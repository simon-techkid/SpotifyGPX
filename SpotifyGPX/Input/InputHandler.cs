using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public class InputHandler
{
    private ISongInput SongInput { get; }
    private IGpsInput GpsInput { get; }

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

    public List<SpotifyEntry> GetAllSongs()
    {
        // Returns unfiltered (all) songs
        return SongInput.GetAllSongs();
    }

    public List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks)
    {
        // Returns filtered songs
        return SongInput.GetFilteredSongs(tracks);
    }

    public List<GPXTrack> GetAllTracks()
    {
        // Return all tracks
        return GpsInput.GetAllTracks();
    }

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

public enum SongFormats
{
    Json, // To create a new format, create an entry here, an import class, and add it to GetSongHandler and songFormats
    JsonReport
}

public enum GpsFormats
{
    Gpx, // To create a new format, create an entry here, an import class, and add it to GetGpsHandler and gpsFormats
    JsonReport
}

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

public interface IGpsInput
{
    List<GPXTrack> GetAllTracks();
    int TrackCount { get; }
    int PointCount { get; }
}
