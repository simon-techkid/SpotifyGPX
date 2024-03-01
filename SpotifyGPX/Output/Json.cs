// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the JSON format.
/// </summary>
public partial class Json : JsonSaveable, IFileOutput, ISaveable, ITransformable
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
        return Pairs.Select(pair => JObject.FromObject(pair.Song)).ToList();
    }

    /// <summary>
    /// The number of songs within this JSON file.
    /// </summary>
    public int Count => Document.Count; // Number of JObjects in list
}
