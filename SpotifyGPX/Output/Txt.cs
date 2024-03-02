// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the TXT format.
/// </summary>
public partial class Txt : IFileOutput
{
    private string?[] Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the TXT format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    public Txt(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    /// <summary>
    /// Creates an array of strings based on a selected variable in each pair.
    /// </summary>
    /// <param name="Pairs">A list of pairs.</param>
    /// <returns>An array of strings, each string containing a single pair.</returns>
    private static string?[] GetDocument(IEnumerable<SongPoint> Pairs)
    {
        // Below are some examples of arrays
        string?[] strings = GetVerbatim(Pairs, pair => pair.ToString()); // Full pair strings
        string?[] Titles = GetVerbatim(Pairs, pair => pair.Song.ToString()); // Song names
        string?[] URIs = GetVerbatim(Pairs, pair => pair.Song.Song_URI); // Song URIs (paste into Spotify desktop in a new playlist)
        string?[] IPs = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_IP); // IP Addresses
        string?[] Platforms = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_Platform); // Device platforms
        string?[] Countries = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_Country); // Countries
        string?[] Accuracies = GetWithoutDuplicates(Pairs, pair => pair.Accuracy.ToString()); // Accuracies
        string?[] StartReasons = GetWithoutDuplicates(Pairs, pair => pair.Song.Song_StartReason); // Start reasons
        string?[] EndReasons = GetWithoutDuplicates(Pairs, pair => pair.Song.Song_EndReason); // End reasons

        return URIs; // Currently returns URI list, but can be changed to your specification
    }

    /// <summary>
    /// Get a variable (selector) out of each pair, sending each to an array.
    /// </summary>
    /// <typeparam name="T">The type of array to return.</typeparam>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="selector">The variable (of a pair) each element of the array will contain.</param>
    /// <returns>An array (without duplicates) of the specified type, each element of which contains a selected variable of a pair.</returns>
    private static T[] GetWithoutDuplicates<T>(IEnumerable<SongPoint> pairs, Func<SongPoint, T> selector)
    {
        // Pass .Distinct() to ensure no duplicate values in returned array
        return pairs.Select(selector).Distinct().ToArray();
    }

    /// <summary>
    /// Get a variable (selector) out of each pair, sending each to an array.
    /// </summary>
    /// <typeparam name="T">The type of array to return.</typeparam>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="selector">The variable (of a pair) each element of the array will contain.</param>
    /// <returns>An array of the specified type, each element of which contains a selected variable of a pair.</returns>
    private static T[] GetVerbatim<T>(IEnumerable<SongPoint> pairs, Func<SongPoint, T> selector)
    {
        // Return all pairs' selected (selector) object as an array
        return pairs.Select(selector).ToArray();
    }

    /// <summary>
    /// Saves this array of strings (excluding null strings) to a TXT file at the provided path.
    /// </summary>
    /// <param name="path">The path where this TXT file will be saved.</param>
    public void Save(string path)
    {
        File.WriteAllLines(path, Document.Where(uri => uri != null)!, OutputEncoding); // Ensure no empty/null lines are created
    }

    /// <summary>
    /// The number of pairs within this XSPF file.
    /// </summary>
    public int Count => Document.Length; // Number of lines
}
