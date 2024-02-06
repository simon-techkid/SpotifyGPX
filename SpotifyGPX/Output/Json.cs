using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Json : IFileOutput
{
    public static bool SupportsMultiTrack => false; // Does this file format allow multiple GPXTracks to be contained?
    private static Formatting Formatting => Formatting.Indented; // Formatting of exporting JSON

    public Json(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private List<JObject> Document { get; }

    private static List<JObject> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair => pair.Song.Json).ToList();
    }

    public void Save(string path)
    {
        string text = JsonConvert.SerializeObject(Document, Formatting);
        File.WriteAllText(path, text);
    }

    public int Count => Document.Count;
}
