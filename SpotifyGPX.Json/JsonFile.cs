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

    private readonly List<SpotifyEntry> SpotifyEntries => JsonContents.Select(track => new SpotifyEntry(track)).ToList();

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXPoint> gpxPoints)
    {
        // Create list of Spotify songs covering the tracked GPX path timeframe
        List<SpotifyEntry> spotifyEntryCandidates = new();

        try
        {
            // Create a dictionary to store the start and end times for each track
            Dictionary<int, (DateTimeOffset, DateTimeOffset)> trackStartEndTimes = new();

            // Calculate start and end times for each track based on GPX points
            foreach (GPXPoint point in gpxPoints)
            {
                int trackIndex = point.TrackMember;

                if (!trackStartEndTimes.ContainsKey(trackIndex))
                {
                    // Initialize start and end times for the track
                    trackStartEndTimes[trackIndex] = (point.Time, point.Time);
                }
                else
                {
                    // Update start and end times as needed
                    if (point.Time < trackStartEndTimes[trackIndex].Item1)
                    {
                        trackStartEndTimes[trackIndex] = (point.Time, trackStartEndTimes[trackIndex].Item2);
                    }
                    if (point.Time > trackStartEndTimes[trackIndex].Item2)
                    {
                        trackStartEndTimes[trackIndex] = (trackStartEndTimes[trackIndex].Item1, point.Time);
                    }
                }
            }

            // Filter Spotify entries based on track-specific start and end times

            return SpotifyEntries
            .Where(entry =>
            {
                // Determine the associated track for each Spotify entry based on its timestamp
                int associatedTrack = -1; // Default value indicating no associated track
                DateTimeOffset entryTime = entry.Time;

                foreach (var trackTimes in trackStartEndTimes)
                {
                    if (entryTime >= trackTimes.Value.Item1 && entryTime <= trackTimes.Value.Item2)
                    {
                        associatedTrack = trackTimes.Key;
                        break; // Exit the loop as soon as an associated track is found
                    }
                }

                // Filter entries associated with a track
                return associatedTrack != -1;
            })
            .Select((song, index) =>
            {
                song.Index = index;
                return song;
            })
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
        }
    }
}