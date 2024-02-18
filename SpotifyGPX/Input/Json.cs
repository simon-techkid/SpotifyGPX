// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Input;

public class Json : ISongInput
{
    private List<SpotifyEntry> AllSongs { get; } // All songs parsed from the JSON

    public Json(string path)
    {
        AllSongs = ParseEntries(path);
    }

    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    public int Count => AllSongs.Count;

    private static List<SpotifyEntry> ParseEntries(string jsonFilePath)
    {
        var serializer = new JsonSerializer();

        using var fileStream = File.OpenRead(jsonFilePath);
        using var jsonReader = new JsonTextReader(new StreamReader(fileStream));
        List<SpotifyEntry> spotifyEntries = new();
        int index = 0;

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                var json = serializer.Deserialize<JObject>(jsonReader);
                if (json != null)
                {
                    spotifyEntries.Add(new SpotifyEntry(index++, json));
                }
            }
        }

        return spotifyEntries;
    }
}