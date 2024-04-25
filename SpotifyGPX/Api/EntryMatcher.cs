// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Api;

/// <summary>
/// A class for matching song entries with their API metadata from the respective API service.
/// </summary>
public class EntryMatcher
{
    private readonly List<ISongEntry> _songs;

    /// <summary>
    /// Creates a new EntryMatcher given a list of <see cref="ISongEntry"/> objects to match.
    /// </summary>
    /// <param name="songs">A list of <see cref="ISongEntry"/> objects to get the API metadata for.</param>
    public EntryMatcher(List<ISongEntry> songs)
    {
        _songs = songs;
    }

    /// <summary>
    /// Match the songs given to the initialized <see cref="EntryMatcher(List{ISongEntry})"/> with their respective API metadata.
    /// </summary>
    /// <returns></returns>
    public List<ISongEntry> MatchEntries()
    {
        // Filter out null values and cast to ISpotifyApiCompat
        List<ISpotifyApiCompat> validSpotifySongs = _songs
            .OfType<ISpotifyApiCompat>()
            .ToList();

        // Extract track IDs
        string[] trackIds = validSpotifySongs.Select(s => s.SongID).ToArray();

        // Get metadata from Spotify API
        var metadatas = SpotifyApiHandler.GetAllEntries(trackIds);

        // Update metadata for valid songs
        foreach (ISpotifyApiCompat song in validSpotifySongs)
        {
            if (metadatas.TryGetValue(song.SongID, out SpotifyApiEntry metadata))
            {
                song.Metadata = metadata;
            }
            else
            {
                Console.WriteLine($"[API] No metadata found for {song.Song_Name} ({song.SongID})");
            }
        }

        return _songs;
    }
}
