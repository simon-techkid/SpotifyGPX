using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class JsonReport : IFileOutput
{
    public static bool SupportsMultiTrack => true; // Does this file format allow multiple GPXTracks to be contained?
    private static Formatting Formatting => Formatting.Indented; // Formatting of exporting JSON

    public JsonReport(IEnumerable<SongPoint> pairs)
    {
        (List<JObject> doc, int count) = GetDocument(pairs);
        Document = doc;
        Count = count;
    }

    public List<JObject> Document { get; }

    private static (List<JObject>, int) GetDocument(IEnumerable<SongPoint> Pairs)
    {
        int count = 0;
        
        return (
            
            Pairs
            .GroupBy(pair => pair.Origin)
            .Select(track =>
            {
                return new JObject(
                    new JProperty(track.Key.ToString(), track
                    .SelectMany(pair =>
                    {
                        count++;
                        return new JArray(CreateJsonReport(pair));
                    })),
                    new JProperty("Count", track.Count()),
                    new JProperty("Track", ToObject(track.Key))
                );
            })
            .ToList(),

            count);
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
            new JProperty("Latitude", point.Location.Latitude),
            new JProperty("Longitude", point.Location.Longitude),
            new JProperty("Time", point.Time)
        );
    }

    private static JObject ToObject(TrackInfo tInfo)
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

    public int Count { get; }
}
