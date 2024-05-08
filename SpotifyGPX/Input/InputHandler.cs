// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Input;

/// <summary>
/// Handle various input file formats for taking in song and GPS information.
/// </summary>
public partial class InputHandler : IDisposable
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

        // If the song and/or GPS format support hash checking, verify their hashes.
        VerifyAllHashes();

        Console.WriteLine($"[INP] Parsed {GpsInput.ParsedTrackCount}/{GpsInput.SourceTrackCount} tracks and {GpsInput.ParsedPointCount}/{GpsInput.SourcePointCount} points from '{Path.GetFileName(gpsPath)}'");
        Console.WriteLine($"[INP] Parsed {SongInput.ParsedSongCount}/{SongInput.SourceSongCount} songs from '{Path.GetFileName(songPath)}'");
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

        // If the pairings format supports hash checking, verify their hashes.
        VerifyAllHashes();

        Console.WriteLine($"[INP] Parsed {PairInput.ParsedPairCount}/{PairInput.SourcePairCount} pairs from '{Path.GetFileName(pairPath)}'");
    }

    /// <summary>
    /// Gets all song records from the given file.
    /// </summary>
    /// <returns>A list of SpotifyEntries, each representing a single song of playback.</returns>
    public List<ISongEntry> GetAllSongs()
    {
        return SongInput.GetAllSongs();
    }

    /// <summary>
    /// Filters the songs in the file by ensuring the returned songs' <see cref="ISongEntry.Time"/> fall within the provided <see cref="GpsTrack"/> objects.
    /// </summary>
    /// <param name="tracks">The tracks whose <see cref="GpsTrack.Start"/> and <see cref="GpsTrack.End"/> will filter the <see cref="ISongEntry.Time"/>.</param>
    /// <returns></returns>
    public List<ISongEntry> GetFilteredSongs(List<GpsTrack> tracks)
    {
        return SongInput.GetFilteredSongs(tracks);
    }

    /// <summary>
    /// Gets all journey tracks from the given file.
    /// </summary>
    /// <returns>A list of GPXTracks, each representing a collection of positions comprising a journey's path.</returns>
    public List<GpsTrack> GetAllTracks()
    {
        return GpsInput.GetAllTracks();
    }

    /// <summary>
    /// Gets user-selected journey track(s) from the given file.
    /// </summary>
    /// <returns>A list of GPXTrack(s) based on user selection, each representing a collection of positions comprising a journey's path</returns>
    public List<GpsTrack> GetSelectedTracks()
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
            throw new Exception($"Unable to get pairs: no provided input file supports pair records.");
        }
    }

    /// <summary>
    /// Verify all hashes for provided files.
    /// </summary>
    private void VerifyAllHashes()
    {
        RunHashVerificationForInput(SongInput);
        RunHashVerificationForInput(GpsInput);
        if (PairInput != null)
        {
            RunHashVerificationForInput(PairInput);
        }
    }

    public void Dispose()
    {
        SongInput.Dispose();
        GpsInput.Dispose();
        PairInput?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verify the hash of an input format, if it implements IHashVerifier.
    /// </summary>
    /// <param name="inputClass">The input class for the format in question.</param>
    private static void RunHashVerificationForInput(object inputClass)
    {
        if (inputClass is IHashVerifier hashVerifier)
        {
            bool hashVerified = hashVerifier.VerifyHash();

            if (hashVerified)
            {
                Console.WriteLine($"Hash verification successful for {inputClass.GetType().Name}.");
            }
            else
            {
                Console.WriteLine($"Hash verification failed for {inputClass.GetType().Name}.");
            }
        }
    }
}
