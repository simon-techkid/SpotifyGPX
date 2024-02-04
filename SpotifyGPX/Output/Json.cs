using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Json : FormatHandler.IFileOutput
{
    public static bool SupportsMultiTrack => false;
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
    }

    public int Count => Document.Count;
}
