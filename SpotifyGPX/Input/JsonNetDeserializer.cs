// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SpotifyGPX.Input;

public class JsonNetDeserializer
{
    private string JsonPath { get; }

    public JsonNetDeserializer(string path)
    {
        JsonPath = path;
    }

    public List<T> Deserialize<T>(JsonSerializerOptions options)
    {
        using FileStream fs = new(JsonPath, FileMode.Open);
        using StreamReader sr = new(fs);
        string jsonString = sr.ReadToEnd();

        List<T> objects = JsonSerializer.Deserialize<List<T>>(jsonString, options) ?? new List<T>();

        return objects;
    }

}
