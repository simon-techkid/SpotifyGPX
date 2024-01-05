// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable

namespace SpotifyGPX.Json;

public readonly struct JsonFile
{
    private readonly string jsonFilePath;
    private readonly List<JObject> JsonContents => JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFilePath));

    public JsonFile(string path) => jsonFilePath = path;

    private readonly List<SpotifyEntry> SpotifyEntries => JsonContents.Select((track, index) => new SpotifyEntry(track, index)).ToList();

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXPoint> gpxPoints)
    {
        // Create list of Spotify songs covering the tracked GPX path timeframe
        List<SpotifyEntry> spotifyEntryCandidates = new();

        try
        {
            // Create a dictionary to store the start and end times for each track            
            var trackStartEndTimes = gpxPoints
            .GroupBy(point => point.TrackMember)
            .ToDictionary(
                group => group.Key,
                group => group.Aggregate(
                    (startTime: group.First().Time, endTime: group.First().Time),
                    (acc, point) => (
                        startTime: point.Time < acc.startTime ? point.Time : acc.startTime,
                        endTime: point.Time > acc.endTime ? point.Time : acc.endTime
                    )
                )
            );

            // Filter Spotify entries based on track-specific start and end times
            return SpotifyEntries
            .Where(entry => // For every song in the entire JSON:
            {
                DateTimeOffset entryTime = entry.Time; // get its time

                return trackStartEndTimes.Any(trackTimes => // Check if the song falls within the GPX tracking time
                    entryTime >= trackTimes.Value.startTime && entryTime <= trackTimes.Value.endTime);
            })
            .ToList(); // Send all the relevant songs to a list!

        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
        }
    }
}