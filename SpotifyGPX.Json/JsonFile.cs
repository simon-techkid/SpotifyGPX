// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace SpotifyGPX.Json;

public readonly struct JsonFile
{
    private readonly string jsonFilePath;

    private readonly List<JObject> JsonContents => JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFilePath)) ?? throw new System.Exception($"JSON deserialization results in null, check the JSON");

    public JsonFile(string path) => jsonFilePath = path;

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXTrack> tracks)
    {
        List<SpotifyEntry> allSongs = JsonContents.Select((track, index) => new SpotifyEntry(track, index)).ToList();

        // Use ConcurrentDictionary to store songs for each track index
        ConcurrentDictionary<int, List<SpotifyEntry>> songsByTrackIndex = new();

        Parallel.ForEach(tracks, track =>
        {
            Console.WriteLine(track);

            // Collect songs using parallel processing
            List<SpotifyEntry> songsInRange = allSongs
                .Where(entry => (entry.Time >= track.Start) && (entry.Time <= track.End))
                .ToList();

            int trackIndex = tracks.IndexOf(track);

            // Add songs to ConcurrentDictionary with the track index
            songsByTrackIndex.AddOrUpdate(trackIndex, songsInRange, (_, existingSongs) =>
            {
                existingSongs.AddRange(songsInRange);
                return existingSongs;
            });
        });

        // Extract and sort songs based on the original track index order
        List<SpotifyEntry> filteredSongs = Enumerable.Range(0, tracks.Count)
            .SelectMany(index => songsByTrackIndex.TryGetValue(index, out List<SpotifyEntry>? value) ? value : new List<SpotifyEntry>())
            .ToList();

        return filteredSongs;
    }
}