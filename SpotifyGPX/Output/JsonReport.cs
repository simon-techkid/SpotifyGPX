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
                    new JProperty("TrackInfo", ToJObject(track.Key)), // Include info about the GPX track
                    new JProperty(track.Key.ToString(), track
                    .SelectMany(pair =>
                    {
                        return new JArray(CreateJsonReport(pair)); // Create a json report for each pair
                    }))
                );
            })
            .ToList();
    }

    private static JObject CreateJsonReport(SongPoint pair)
    {
        return new JObject(
            new JProperty("Index", pair.Index), // Index of this pairing
            new JProperty("SpotifyEntry", ToJObject(pair.Song)), // Spotify entry index, original Json, time, duration, offline timestamp
            new JProperty("GPXPoint", ToJObject(pair.Point)), // GPX point index, lat, lon, time
            new JProperty("NormalizedOffset", pair.NormalizedOffset), // UTC offset
            new JProperty("PointTime", pair.PointTime.UtcDateTime.ToString(Options.GpxOutput)),
            new JProperty("Accuracy", pair.Accuracy), // Accuracy between point and song time
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
