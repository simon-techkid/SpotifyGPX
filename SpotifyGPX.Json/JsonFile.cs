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

            /* TODO:
             * Widen the search range of songs by subtracting msPlayed (first song) from gpxStartTime 
             * This will allow songs that started playing before the GPX started, be included
             * Don't necessarily need to implement in Release, but would be a good test
             * Check its accuracy before deciding permanence
            */

            // Filter Spotify entries based on track-specific start and end times
            return SpotifyEntries
            .Where(entry => // For every song in the entire JSON:
            {
                DateTimeOffset entryTime = entry.Time; // get the song's played time

                // Return true if the song falls inside the GPX track
                return trackStartEndTimes.Any(trackTimes =>
                    entryTime >= trackTimes.Value.startTime && entryTime <= trackTimes.Value.endTime); // Song played between start & end of the GPX
            })
            .ToList(); // Send all the relevant songs to a list!

        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
        }
    }
}