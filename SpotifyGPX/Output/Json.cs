using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Json
{
    private static Formatting Formatting => Formatting.Indented; // Formatting of exporting JSON

    public Json(IEnumerable<SongPoint> pairs) => Document = GetJObjects(pairs);

    private List<JObject> Document { get; }

    private static List<JObject> GetJObjects(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair => pair.Song.Json).ToList();
    }

    public void Save(string path)
    {
        string text = JsonConvert.SerializeObject(Document, Formatting);
        File.WriteAllText(path, text);
        Console.WriteLine(ToString());
    }

    private int Count => Document.Count;

    public override string ToString() => $"[FILE] JSON file containing {Count} points saved!";
}
