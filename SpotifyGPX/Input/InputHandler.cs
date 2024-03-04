// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;

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

        SongInput = CreateSongInput(pairPath);
        GpsInput = CreateGpsInput(pairPath);
        PairInput = CreatePairInput(pairPath);

        Console.WriteLine($"[INP] '{Path.GetFileName(pairPath)}' contains {PairInput.PairCount} total pairs");
    }

    /// <summary>
    /// Gets all song records from the given file.
    /// </summary>
    /// <returns>A list of SpotifyEntries, each representing a single song of playback.</returns>
    public List<SpotifyEntry> GetAllSongs()
    {
        // Returns unfiltered (all) songs
        if (SongInput != null)
        {
            return SongInput.GetAllSongs();
        }
        else
        {
            throw new Exception($"Unable to get pairs: this input format does not support pairs.");
        }
    }

    /// <summary>
    /// Filters the song records from the given file.
    /// </summary>
    /// <param name="tracks">A list of GPXTracks, by which the contents of the song record list will be filtered.</param>
    /// <returns>A list of SpotifyEntries, each representing a single song of playback.</returns>
    public List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks)
    {
        return SongInput.GetFilteredSongs(tracks);
    }

    /// <summary>
    /// Gets all journey tracks from the given file.
    /// </summary>
    /// <returns>A list of GPXTracks, each representing a collection of positions comprising a journey's path.</returns>
    public List<GPXTrack> GetAllTracks()
    {
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
            ".xspf" => new Xspf(path),
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

    /// <summary>
    /// Determines the appropriate import class for handling this Song-Point pairings file.
    /// </summary>
    /// <param name="path">The path to the Song-Point pairings file.</param>
    /// <returns>An IPairInput interface allowing interfacing with the corresponding format.</returns>
    /// <exception cref="Exception">The provided file doesn't have an inport class associated with it.</exception>
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
