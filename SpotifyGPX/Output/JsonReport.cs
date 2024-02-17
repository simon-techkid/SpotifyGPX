// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class JsonReport : IFileOutput
{
    private static Formatting Formatting => Formatting.Indented; // Formatting of exporting JSON

    public JsonReport(IEnumerable<SongPoint> pairs)
    {
        Document = GetDocument(pairs);
    }

    public List<JObject> Document { get; }

    private static List<JObject> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        return Pairs
            .GroupBy(pair => pair.Origin) // Group the pairs by track (JsonReport supports multiTrack)
            .Select(track =>
            {
                return new JObject(
                    new JProperty("Count", track.Count()), // Include # of pairs in this track
                    new JProperty("TrackInfo", JToken.FromObject(track.Key)), // Include info about the GPX track
                    new JProperty(track.Key.ToString(), track
                    .SelectMany(pair =>
                    {
                        return new JArray(JToken.FromObject(pair)); // Create a json report for each pair
                    }))
                );
            })
            .ToList();
    }

    public void Save(string path)
    {
        string text = JsonConvert.SerializeObject(Document, Formatting);
        File.WriteAllText(path, text);
    }

    public int Count
    {
        get
        {
            // For each document (JObject) in Document (List<JObject>),
            // Get that JObject's children
            // Select the last child (in this case, the pair list)
            // Get the count of pairs within the pair list
            // Get the sum of pairs in that JObject
            // Get the sum of pairs in all selected JObjects of List<JObject>

            return Document.Select(JObject => JObject.Children().Last().Select(pair => pair.Count()).Sum()).Sum();
        }
    }
}
