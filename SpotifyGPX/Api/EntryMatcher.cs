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
        // Filter out null values from _songs before passing to the GetAllEntries method
        string[] trackIds = _songs.Where(s => (s is SpotifyEntry spotifySong) && spotifySong.Song_ID != null)
                                  .Select(s => ((SpotifyEntry)s).Song_ID)
                                  .ToArray();

        // Ensure trackIds is not null before passing it to GetAllEntries
        Dictionary<string, SpotifyApiEntry> metadatas = SpotifyApiHandler.GetAllEntries(trackIds);

        return _songs.Where(song => (song is SpotifyEntry spotifySong) && spotifySong.Song_ID != null).Select(song =>
        {
            SpotifyEntry spotifySong = (SpotifyEntry)song; // Cast to SpotifyEntry to access Spotify-specific properties
            if (metadatas.TryGetValue(spotifySong.Song_ID, out SpotifyApiEntry metadata))
            {
                spotifySong.Metadata = metadata;
            }
            else
            {
                Console.WriteLine($"[API] No metadata found for {spotifySong.Song_Name} ({spotifySong.Song_ID})");
            }
            return song;
        }).ToList();
    }
}
