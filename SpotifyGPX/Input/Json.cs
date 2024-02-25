// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing song playback data from the JSON format.
/// </summary>
public class Json : ISongInput
{
    private List<SpotifyEntry> AllSongs { get; } // All songs parsed from the JSON

    /// <summary>
    /// Creates a new input handler for handling files in the JSON format.
    /// </summary>
    /// <param name="path">The path of the JSON file.</param>
    public Json(string path)
    {
        AllSongs = ParseEntries(path);
    }

    /// <summary>
    /// Gets all the songs, as a list, from the JSON file.
    /// </summary>
    /// <returns>A list of all the SpotifyEntries in the JSON.</returns>
    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    /// <summary>
    /// The total number of songs contained in the JSON file.
    /// </summary>
    public int SongCount => AllSongs.Count;

    /// <summary>
    /// Parses a JSON file to a list of song records.
    /// </summary>
    /// <param name="jsonFilePath">The path to a JSON file containing Spotify playback data</param>
    /// <returns>A list of SpotifyEntry objects, each representing song playback information.</returns>
    /// <exception cref="Exception"></exception>
    private static List<SpotifyEntry> ParseEntries(string jsonFilePath)
    {
        List<SpotifyEntry> spotifyEntries = new(); // List of SpotifyEntries to be returned
        int index = 0; // Index of the current entry

        // Create a serializer with the settings from Options.JsonSettings
        JsonSerializer serializer = JsonSerializer.Create(Options.JsonSettings);

        // Open the file stream and create a JsonTextReader
        using (var fileStream = File.OpenRead(jsonFilePath)) // Open the file stream
        using (var streamReader = new StreamReader(fileStream)) // Create a StreamReader from the file stream
        using (var jsonReader = new JsonTextReader(streamReader)) // Create a JsonTextReader from the StreamReader
        {
            // Read JSON objects from the file
            while (jsonReader.Read())
            {
                // Check if the current token is the start of an object
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    JObject? json = serializer.Deserialize<JObject>(jsonReader); // Deserialize the JSON object

                    if (json != null) // If deserialization is successful, add a new SpotifyEntry to the list
                    {
                        spotifyEntries.Add(new SpotifyEntry(index++, json));
                    }
                    else // If deserialization is unsuccessful, throw an exception
                    {
                        throw new Exception($"Input file contains null JSON entries on top level entry {index++}");
                    }
                }
            }
        }

        return spotifyEntries;
    }
}