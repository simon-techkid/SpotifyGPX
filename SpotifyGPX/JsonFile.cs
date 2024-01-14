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

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXTrack> tracks)
    {
        var trackRange = tracks.Select(track => (track.Start, track.End)).ToList(); // List all of the tracks' start and end times
        List<SpotifyEntry> allSongs = JsonContents.Select((json, index) => new SpotifyEntry(index, json)).ToList();

        return allSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => // Starts after the beginning of the GPX, and before the end of the GPX
                spotifyEntry.Time >= trackTimes.Start && spotifyEntry.Time <= trackTimes.End))
            .ToList(); // Send the songs that fall within GPX tracking period to a list
    }
}