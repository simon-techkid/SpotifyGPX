// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace SpotifyGPX.Json;

public readonly struct JsonFile
{
    private readonly string jsonFilePath;

    private readonly List<JObject> JsonContents => ReadJsonContentsAsync(jsonFilePath).Result;

    private static async Task<List<JObject>> ReadJsonContentsAsync(string jsonFilePath)
    {
        string json = await Task.Run(() => File.ReadAllTextAsync(jsonFilePath));
        return await Task.Run(() => JsonConvert.DeserializeObject<List<JObject>>(json));
    }

    public JsonFile(string path) => jsonFilePath = path;

    private readonly List<SpotifyEntry> SpotifyEntries => JsonContents.Select((track, index) => new SpotifyEntry(track, index)).ToList();

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXPoint> gpxPoints)
    {
        try
        {
            // Get start and end times for each GPX track
            var trackStartEndTimes = gpxPoints // Returns track number, start time of the track, and end time of the track
            .GroupBy(point => point.TrackMember) // Distinguish which track each point came from
            .ToDictionary(
                group => group.Key, // Dictionary key is the track member
                group => group.Aggregate(
                    (startTime: group.First().Time, endTime: group.First().Time),
                    (earliest, point) => (
                        startTime: point.Time < earliest.startTime ? point.Time : earliest.startTime,
                        endTime: point.Time > earliest.endTime ? point.Time : earliest.endTime
                    )
                )
            );

            // Filter Spotify entries based on track-specific start and end times
            return SpotifyEntries
            .Where(entry => trackStartEndTimes.Any(trackTimes => // Return true if the song falls inside the GPX track
                entry.Time >= trackTimes.Value.startTime && entry.Time <= trackTimes.Value.endTime)) // Song played between start & end of the GPX
            .ToList(); // Send all the relevant songs to a list!

        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
        }
    }
}