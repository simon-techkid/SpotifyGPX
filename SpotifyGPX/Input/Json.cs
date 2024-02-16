// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public class Json : ISongInput
{
    private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time
    private static bool ExcludeSkipped => false; // Ignore songs skipped by the user, as defined by Spotify JSON
    private List<SpotifyEntry> AllSongs { get; } // All songs parsed from the JSON

    public Json(string path)
    {
        AllSongs = ParseEntries(path);
    }

    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    public List<SpotifyEntry> GetFilteredSongs(List<GPXTrack> tracks)
    {
        return FilterEntries(tracks);
    }

    public int Count => AllSongs.Count;

    private static List<SpotifyEntry> ParseEntries(string jsonFilePath)
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

    private List<SpotifyEntry> FilterEntries(List<GPXTrack> tracks)
    {
        var trackRange = tracks.Select(track => (track.Start, track.End)).ToList();

        // FilterEntries() differs from AllSongs because it filters the entire JSON file by the following parameters:
        // The song must have been played during the GPS tracking timeframe (but PairingsHandler.PairPoints() filters this too)
        // The song must have been played for longer than the MinimumPlaytime TimeSpan (beginning of this file)
        // The song must have not been skipped during playback by the user (if ExcludeSkipped is true)
        
        // You may add other filtration options below, within the .Any() statement:

        return AllSongs.Where(spotifyEntry => // If the spotify entry
            trackRange.Any(trackTimes => // Starts after the beginning of the GPX, and before the end of the GPX
                spotifyEntry.WithinTimeFrame(trackTimes.Start, trackTimes.End) && // Exclude song if played outside the time range of tracks
                (spotifyEntry.TimePlayed != null ? spotifyEntry.TimePlayed >= MinimumPlaytime : true) && // Exclude song if played for shorter time than options specifies
                (spotifyEntry.Song_Skipped == true && ExcludeSkipped ? false : true))) // Exclude song if skipped and if options specifies to exclude
            .ToList(); // Send the songs that fall within GPX tracking period to a list
    }
}