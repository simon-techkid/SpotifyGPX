// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing song playback data from the JSON format.
/// </summary>
public partial class Json : ISongInput, IJsonDeserializer
{
    private JsonDeserializer JsonDeserializer { get; }
    private List<JObject> AllEntries { get; }
    private List<SpotifyEntry> AllSongs { get; } // All songs parsed from the JSON

    /// <summary>
    /// Creates a new input handler for handling files in the JSON format.
    /// </summary>
    /// <param name="path">The path of the JSON file.</param>
    public Json(string path)
    {
        JsonDeserializer = new JsonDeserializer(path, JsonSettings);
        AllEntries = Deserialize();
        AllSongs = ParseEntriesToSongs();
    }

    public List<JObject> Deserialize()
    {
        return JsonDeserializer.Deserialize();
    }

    public List<SpotifyEntry> ParseEntriesToSongs()
    {
        return AllEntries.Select((entry, index) => new SpotifyEntry(index, entry)).ToList();
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
}

public class JsonDeserializer : IJsonDeserializer
{
    private string JsonPath { get; }
    private JsonSerializerSettings? SerializerSettings { get; }

    public JsonDeserializer(string path, JsonSerializerSettings? settings)
    {
        JsonPath = path;
        SerializerSettings = settings;
    }

    public List<JObject> Deserialize()
    {
        List<JObject> tracks = new();

        JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);

        using (var fileStream = File.OpenRead(JsonPath))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    var json = serializer.Deserialize<JObject>(jsonReader);

                    if (json != null)
                    {
                        tracks.Add(json);
                    }
                    else
                    {
                        throw new Exception($"Input file contains null JSON entries on top level entry.");
                    }
                }
            }
        }

        return tracks;
    }
}