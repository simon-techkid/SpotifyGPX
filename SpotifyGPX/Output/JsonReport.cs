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
            .GroupBy(pair => pair.Origin)
            .Select(track =>
            {
                return new JObject(
                    new JProperty("Count", track.Count()),
                    new JProperty("TrackInfo", ToJObject(track.Key)),
                    new JProperty(track.Key.ToString(), track
                    .SelectMany(pair =>
                    {
                        return new JArray(CreateJsonReport(pair));
                    }))
                );
            })
            .ToList();
    }

    private static JObject CreateJsonReport(SongPoint pair)
    {
        return new JObject(
            new JProperty("Index", pair.Index),
            new JProperty("SpotifyEntry", ToJObject(pair.Song)),
            new JProperty("GPXPoint", ToJObject(pair.Point)),
            new JProperty("NormalizedOffset", pair.NormalizedOffset),
            new JProperty("PointTime", pair.PointTime.UtcDateTime.ToString(Options.GpxOutput)),
            new JProperty("Accuracy", pair.Accuracy),
            new JProperty("SongTime", pair.SongTime.UtcDateTime.ToString(Options.GpxOutput))
        );
    }

    private static JObject ToJObject(SpotifyEntry song)
    {
        return new JObject(
            new JProperty("Index", song.Index),
            new JProperty("Original", song.Json),
            new JProperty("Time", song.Time.UtcDateTime.ToString(Options.GpxOutput)),
            new JProperty("TimePlayed", song.TimePlayed?.ToString(Options.DescriptionTimePlayed)),
            new JProperty("OfflineTimestamp", song.OfflineTimestamp?.UtcDateTime.ToString(Options.GpxOutput))
        );
    }

    private static JObject ToJObject(GPXPoint point)
    {
        return new JObject(
            new JProperty("Index", point.Index),
            new JProperty("Latitude", point.Location.Latitude),
            new JProperty("Longitude", point.Location.Longitude),
            new JProperty("Time", point.Time.UtcDateTime.ToString(Options.GpxOutput))
        );
    }

    private static JObject ToJObject(TrackInfo tInfo)
    {
        return new JObject(
            new JProperty("Index", tInfo.Index),
            new JProperty("Name", tInfo.Name),
            new JProperty("Type", tInfo.Type.ToString())
        );
    }

    public void Save(string path)
    {
        string text = JsonConvert.SerializeObject(Document, Formatting);
        File.WriteAllText(path, text);
    }

    public int Count => Document.Select(doc => (int)doc["Count"]).Sum();
}
