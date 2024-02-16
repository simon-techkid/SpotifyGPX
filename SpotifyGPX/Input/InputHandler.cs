using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Input;

public class InputHandler
{
    private ISongInput SongInput { get; }
    private IGpsInput GpsInput { get; }

    public InputHandler(string songPath, string gpsPath)
    {
        string songExtension = Path.GetExtension(songPath).ToLower();
        string gpsExtension = Path.GetExtension(gpsPath).ToLower();

        if (!File.Exists(songPath))
        {
            throw new Exception($"The specified file, '{songPath}', does not exist!");
        }

        if (!File.Exists(gpsPath))
        {
            throw new Exception($"The specified file, '{gpsPath}', does not exist!");
        }

        if (!songFormats.TryGetValue(songExtension, out SongFormats songFormat))
        {
            throw new Exception($"The specified file, '{songPath}', is not a valid Spotify data file!");
        }

        if (!gpsFormats.TryGetValue(gpsExtension, out GpsFormats gpsFormat))
        {
            throw new Exception($"The specified file, '{gpsPath}', is not a valid GPS exchange data file!");
        }

        GpsInput = GetGpsHandler(gpsPath)[gpsFormat];
        SongInput = GetSongHandler(songPath)[songFormat];
        Console.WriteLine($"[INP] '{Path.GetFileName(gpsPath)}' contains {GpsInput.TrackCount} tracks and {GpsInput.PointCount} points");
        Console.WriteLine($"[INP] '{Path.GetFileName(songPath)}' contains {SongInput.Count} total songs");
    }

    public List<SpotifyEntry> GetAllSongs()
    {
        return SongInput.GetAllSongs(); // Return all songs contained in the given JSON
    }

    public List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks)
    {
        // List all of the tracks' start and end times
        return SongInput.GetFilteredSongs(tracks); // Return filtered songs based on given JSON
    }

    public List<GPXTrack> GetAllTracks()
    {
        return GpsInput.GetAllTracks(); // Return all tracks contained in the given GPX
    }

    private static readonly Dictionary<string, SongFormats> songFormats = new()
    {
        { ".json", SongFormats.Json } // Define the extensions corresponding to the Spotify formats
    };

    private static readonly Dictionary<string, GpsFormats> gpsFormats = new()
    {
        { ".gpx", GpsFormats.Gpx } // Define the extensions corresponding to the Spotify formats
    };

    private static Dictionary<SongFormats, ISongInput> GetSongHandler(string songPath)
    {
        return new Dictionary<SongFormats, ISongInput> // Each of the below classes inherit ISongInput, as they are format classes sharing methods and fields
        {
            { SongFormats.Json, new Json(songPath) } // Initialize a new Json class with the path provided
        };
    }

    private static Dictionary<GpsFormats, IGpsInput> GetGpsHandler(string gpsPath)
    {
        return new Dictionary<GpsFormats, IGpsInput> // Each of the below classes inherit IGpsInput, as they are format classes sharing methods and fields
        {
            { GpsFormats.Gpx, new Gpx(gpsPath) } // Initialize a new Gpx class with the path provided
        };
    }

}

public enum SongFormats
{
    Json // To create a new format, create an entry here, an import class, and add it to GetSongHandler and songFormats
}

public enum GpsFormats
{
    Gpx // To create a new format, create an entry here, an import class, and add it to GetGpsHandler and gpsFormats
}

public interface ISongInput
{
    List<SpotifyEntry> GetAllSongs();
    List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks);
    int Count { get; }
}

public interface IGpsInput
{
    List<GPXTrack> GetAllTracks();
    int TrackCount { get; }
    int PointCount { get; }
}
