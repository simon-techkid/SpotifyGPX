// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        Dictionary<int, (DateTimeOffset startTime, DateTimeOffset endTime)> trackRange = TrackRanges(tracks);
        List<SpotifyEntry> allSongs = JsonContents.Select((json, index) => new SpotifyEntry(index, json)).ToList();

        // Filter Spotify entries based on track-specific start and end times
        return allSongs
        .Where(entry => trackRange.Any(trackTimes => // Return true if the song falls inside the GPX track
            (entry.Time >= trackTimes.Value.startTime) && (entry.Time <= trackTimes.Value.endTime))) // Song played between start & end of the GPX
        .ToList(); // Send all the relevant songs to a list!
    }

    public readonly Dictionary<int, (DateTimeOffset startTime, DateTimeOffset endTime)> TrackRanges(List<GPXTrack> gpxTracks)
    {
        // Get start and end times for each GPX track
        return gpxTracks // Returns track number, start time of the track, and end time of the track
            .SelectMany(track => track.Points) // Flatten List<GPXTrack> to List<GPXPoint>
            .GroupBy(point => point.TrackIndex) // Distinguish which track each point came from
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
    }

}