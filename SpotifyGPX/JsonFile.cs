// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX;

public readonly struct JsonFile
{
    private readonly string jsonFilePath;

    private readonly List<JObject> JsonContents => JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFilePath)) ?? throw new System.Exception($"JSON deserialization results in null, check the JSON");

    public JsonFile(string path) => jsonFilePath = path;

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXTrack> gpxPoints)
    {
        List<SpotifyEntry> allSongs = JsonContents.Select((json, index) => new SpotifyEntry(index, json)).ToList();

        return gpxPoints.SelectMany(track =>
        {
            // Filter Spotify entries based on track-specific start and end times
            return allSongs
            .Where(entry => (entry.Time >= track.Start) && (entry.Time <= track.End)) // Song played between start & end of the GPX
            .ToList(); // Send all the relevant songs to a list!
        })
        .ToList();
    }
}