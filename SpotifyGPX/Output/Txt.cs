// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public sealed partial class Txt : TxtSaveable
{
    public override string FormatName => nameof(Txt).ToLower();
    protected override DocumentAccessor SaveAction => GetDocument;

    public Txt(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

    private string?[] GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> Pairs = DataProvider();

        // Below are some examples of arrays
        string?[] strings = GetVerbatim(Pairs, pair => pair.ToString()); // Full pair strings
        string?[] Accuracies = GetWithoutDuplicates(Pairs, pair => pair.Accuracy.ToString()); // Accuracies
        string?[] Titles = GetVerbatim(Pairs, pair => pair.Song.ToString()); // Song names

        // Some SpotifyEntry specific objects to create your text file with:
        Func<SpotifyEntry, object?> uris = spotifySong => spotifySong.Song_URI;
        Func<SpotifyEntry, object?> ips = spotifySong => spotifySong.Spotify_IP;
        Func<SpotifyEntry, object?> platform = spotifySong => spotifySong.Spotify_Platform;
        Func<SpotifyEntry, object?> country = spotifySong => spotifySong.Spotify_Country;
        Func<SpotifyEntry, object?> startReason = spotifySong => spotifySong.Song_StartReason;
        Func<SpotifyEntry, object?> endReason = spotifySong => spotifySong.Song_EndReason;

        string?[] lines = Pairs
            .Select(pair => pair.Song
                .GetPropertyValue(uris)?
                .ToString())
            .ToArray();

        string?[] duplicateFiltered = GetWithoutDuplicates(lines); // Select this if you don't want any duplicate value lines in your text file.
        string[] nullOrEmptyFiltered = GetWithoutNullOrEmpty(lines); // Select this if you don't want any null or empty lines in your text file.
        string[] bothFiltered = GetWithoutNullOrEmpty(GetWithoutDuplicates(lines)); // Select this if you don't want any null, empty, or duplicate lines in your text file.

        return lines; // Currently returns URI list, but can be changed to your specification
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
        return GetWithoutDuplicates(pairs.Select(selector).ToArray());
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
        return pairs
            .Select(selector)
            .ToArray();
    }

    /// <summary>
    /// Get an array containing all unique values (no duplicate elements) from the provided array.
    /// </summary>
    /// <typeparam name="T">The type <typeparamref name="T"/> of the given array.</typeparam>
    /// <param name="array">The array to filter for duplicates.</param>
    /// <returns>An array of type <typeparamref name="T"/> containing no duplicate values.</returns>
    private static T[] GetWithoutDuplicates<T>(T[] array)
    {
        return array
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Get an array containing all non-null and non-empty values from the provided array.
    /// </summary>
    /// <param name="array">An array containing possibly null elements.</param>
    /// <returns>A string[] containing no null or empty values.</returns>
    private static string[] GetWithoutNullOrEmpty(string?[] array)
    {
        return array
            .Where(element => !string.IsNullOrEmpty(element))
            .ToArray()!; // Explicitly convert to non-nullable array
    }

    public override int Count => Document.Length;
}
