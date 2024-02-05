using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class JsonReport : OutputHandler.IFileOutput
{
    public static bool SupportsMultiTrack => true;
    private static Formatting Formatting => Formatting.Indented; // Formatting of exporting JSON

    public JsonReport(IEnumerable<SongPoint> pairs) => Document = GetJsonReport(pairs);

    public List<JObject> Document { get; }

    private static List<JObject> GetJsonReport(IEnumerable<SongPoint> Pairs)
    {
        return Pairs
            .GroupBy(pair => pair.Origin)
            .Select(track =>
            {
                return new JObject(
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
            new JProperty("Accuracy", pair.Accuracy),
            new JProperty("NormalizedOffset", pair.NormalizedOffset),
            new JProperty("SongTime", pair.SongTime),
            new JProperty("PointTime", pair.PointTime)
        );
    }

    private static JObject ToJObject(SpotifyEntry song)
    {
        return new JObject(
            new JProperty("Index", song.Index),
            new JProperty("Original", song.Json),
            new JProperty("Time", song.Time),
            new JProperty("TimePlayed", song.TimePlayed),
            new JProperty("OfflineTimestamp", song.OfflineTimestamp)
        );
    }

    private static JObject ToJObject(GPXPoint point)
    {
        return new JObject(
            new JProperty("Index", point.Index),
            new JProperty("lat", point.Location.Latitude),
            new JProperty("lon", point.Location.Longitude),
            new JProperty("time", point.Time)
        );
    }

    public void Save(string path)
    {
        string text = JsonConvert.SerializeObject(Document, Formatting);
        File.WriteAllText(path, text);
    }

    public int Count => Document.SelectMany(tracks => tracks.Properties().SelectMany(track => track.Value.Children())).Count();
}
