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
    private readonly List<SpotifyEntry> _songs;

    /// <summary>
    /// Creates a new EntryMatcher given a list of <see cref="SpotifyEntry"/> objects to match.
    /// </summary>
    /// <param name="songs">A list of <see cref="SpotifyEntry"/> objects to get the API metadata for.</param>
    public EntryMatcher(List<SpotifyEntry> songs)
    {
        _songs = songs;
    }

    /// <summary>
    /// Match the songs given to the initialized <see cref="EntryMatcher(List{SpotifyEntry})"/> with their respective API metadata.
    /// </summary>
    /// <returns></returns>
    public List<SpotifyEntry> MatchEntries()
    {
        Dictionary<string, SpotifyApiEntry> metadatas = SpotifyApiHandler.GetAllEntries(_songs.Select(s => s.Song_ID).ToArray());

        return _songs.Where(song => song.Song_ID != null).Select(song =>
        {
            if (metadatas.TryGetValue(song.Song_ID, out SpotifyApiEntry metadata))
            {
                song.Metadata = metadata;
            }
            else
            {
                Console.WriteLine($"[INP] No metadata found for {song.Song_Name} ({song.Song_ID})");
            }
            return song;
        }).ToList();
    }
}
