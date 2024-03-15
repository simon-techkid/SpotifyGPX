// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the JSON format.
/// </summary>
public partial class Json : JsonSaveable
{
    protected override List<JObject> Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the JSON format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    public Json(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    /// <summary>
    /// Creates a JSON document (a list of JObjects) representing pairs' songs.
    /// </summary>
    /// <param name="Pairs">A list of pairs.</param>
    /// <returns>A list of JObjects, each representing the original Spotify playback JSON from each pair.</returns>
    private static List<JObject> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair =>
        {
            JObject songJson = JObject.FromObject(pair.Song);

            // Uncomment below to return unchanged:
            //return songJson;

            // SpotifyGPX properties' leading name:
            string prefix = "SGPX_";

            // Remove non-Spotify properties
            songJson.Properties() // For all the properties in the JObject,
                .Where(property => property.Name.StartsWith(prefix)) // If the name indicates it is non-Spotify:
                .ToList() // Send non-Spotify properties to a list
                .ForEach(removeThis => removeThis.Remove()); // Remove non-Spotify properties.

            return songJson;

        }).ToList();
    }

    /// <summary>
    /// The number of songs within this JSON file.
    /// </summary>
    public override int Count => Document.Count; // Number of JObjects in list
}
