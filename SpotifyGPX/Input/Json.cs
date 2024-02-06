﻿// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public class Json
{
    public Json(string path) => AllSongs = JsonToSpotifyEntry(path);

    public List<SpotifyEntry> AllSongs { get; }

    private static List<SpotifyEntry> JsonToSpotifyEntry(string jsonFilePath)
    {
        var serializer = new JsonSerializer();

        using var fileStream = File.OpenRead(jsonFilePath);
        using var jsonReader = new JsonTextReader(new StreamReader(fileStream));
        List<SpotifyEntry> spotifyEntries = new();
        int index = 0;

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                var json = serializer.Deserialize<JObject>(jsonReader);
                if (json != null)
                {
                    spotifyEntries.Add(new SpotifyEntry(index++, json));
                }
            }
        }

        return spotifyEntries;
    }

    public List<SpotifyEntry> FilterSpotifyJson(List<GPXTrack> gpxTracks)
    {
        var trackRange = gpxTracks.Select(track => (track.Start, track.End)).ToList(); // List all of the tracks' start and end times

        return AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => // Starts after the beginning of the GPX, and before the end of the GPX
                spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End) && // Exclude song if played outside the time range of tracks
                (spotifyEntry.TimePlayed != null ? spotifyEntry.TimePlayed >= Options.MinimumPlaytime : true) && // Exclude song if played for shorter time than options specifies
                (spotifyEntry.Song_Skipped == true && Options.ExcludeSkipped ? false : true))) // Exclude song if skipped and if options specifies to exclude
            .ToList(); // Send the songs that fall within GPX tracking period to a list
    }
}