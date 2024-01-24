// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX;

public readonly struct JsonFile
{
    public JsonFile(string path) => AllSongs = JsonToSpotifyEntry(path);

    public List<SpotifyEntry> AllSongs { get; }

    private static List<SpotifyEntry> JsonToSpotifyEntry(string jsonFilePath) => DeserializeJson(jsonFilePath).Select((json, index) => new SpotifyEntry(index, json)).ToList();

    private static List<JObject> DeserializeJson(string jsonFilePath) => JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFilePath)) ?? throw new System.Exception($"JSON deserialization results in null, check the JSON");

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXTrack> tracks)
    {
        var trackRange = tracks.Select(track => (track.Start, track.End)).ToList(); // List all of the tracks' start and end times

        return AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => // Starts after the beginning of the GPX, and before the end of the GPX
                spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End)))
            .ToList(); // Send the songs that fall within GPX tracking period to a list
    }
}