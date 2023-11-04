// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Json;

public partial class Json
{
    public static List<SpotifyEntry> FilterSpotifyJson(List<SpotifyEntry> spotifyEntries, List<GPXPoint> gpxPoints)
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
            spotifyEntryCandidates = spotifyEntries
            .Where(entry =>
            {
                // Determine the associated track for each Spotify entry based on its timestamp
                int associatedTrack = -1; // Default value indicating no associated track
                DateTimeOffset entryTime = entry.Time_End;

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
            .Select((item, index) =>
            {
                item.Index = index;
                return item;
            })
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
        }

        return spotifyEntryCandidates;
    }
}
