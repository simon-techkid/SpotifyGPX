// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Txt : IFileOutput
{
    public Txt(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private string?[] Document { get; }

    private static string?[] GetDocument(IEnumerable<SongPoint> Pairs)
    {
        // Below are some examples of arrays
        string?[] URIs = GetVerbatim(Pairs, pair => pair.Song.Song_URI); // Song URIs (paste into Spotify desktop in a new playlist)
        string?[] IPs = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_IP); // IP Addresses
        string?[] Platforms = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_Platform); // Device platforms
        string?[] Countries = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_Country); // Countries

        return URIs; // Currently returns URI list, but can be changed to your specification
    }

    private static T[] GetWithoutDuplicates<T>(IEnumerable<SongPoint> pairs, Func<SongPoint, T> selector)
    {
        // Pass .Distinct() to ensure no duplicate values in returned array
        return pairs.Select(selector).Distinct().ToArray();
    }

    private static T[] GetVerbatim<T>(IEnumerable<SongPoint> pairs, Func<SongPoint, T> selector)
    {
        // Return all pairs' selected (selector) object as an array
        return pairs.Select(selector).ToArray();
    }

    public void Save(string path)
    {
        File.WriteAllLines(path, Document.Where(uri => uri != null)!); // Ensure no empty/null lines are created
    }

    public int Count => Document.Length; // Number of lines
}
