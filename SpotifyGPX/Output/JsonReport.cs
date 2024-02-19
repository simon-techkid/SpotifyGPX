// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the JsonReport format.
/// </summary>
public class JsonReport : IFileOutput
{
    private static Formatting Formatting => Formatting.Indented; // Formatting of exporting JSON
    public List<JObject> Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the JsonReport format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    public JsonReport(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    /// <summary>
    /// Creates a JsonReport document (a list of JObjects) representing tracks and their pairs.
    /// </summary>
    /// <param name="Pairs">A list of pairs.</param>
    /// <returns>A list of JObjects, each representing a track containing pairs.</returns>
    private static List<JObject> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        // Create a serializer with the settings from Options.JsonSettings
        JsonSerializer serializer = JsonSerializer.Create(Options.JsonSettings);

        return Pairs
            .GroupBy(pair => pair.Origin) // Group the pairs by track (JsonReport supports multiTrack)
            .Select(track =>
            {
                return new JObject(
                    new JProperty("Count", track.Count()), // Include # of pairs in this track
                    new JProperty("TrackInfo", JToken.FromObject(track.Key)), // Include info about the GPX track
                    new JProperty(track.Key.ToString(), JToken.FromObject(track.SelectMany(pair => new JArray(JToken.FromObject(pair))), serializer)) // Create a json report for each pair
                );
            })
            .ToList();
    }

    /// <summary>
    /// Saves this JsonReport file to the provided path.
    /// </summary>
    /// <param name="path">The path where this JsonReport will be saved.</param>
    public void Save(string path)
    {
        string text = JsonConvert.SerializeObject(Document, Formatting);
        File.WriteAllText(path, text);
    }

    /// <summary>
    /// The number of pairs within this JsonReport file, regardless of track.
    /// </summary>
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
