using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SpotifyGPX
{
    class JSON
    {
        public static List<SpotifyEntry> ParseSpotifyJson(string inputJson)
        {
            List<JObject> jObjects = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(inputJson));

            List<SpotifyEntry> spotifyEntries = jObjects.Select(jObject => new SpotifyEntry
            {
                Time_End = DateTimeOffset.ParseExact((string?)jObject["endTime"], Options.spotifyJsonTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                Song_Artist = (string?)jObject["artistName"],
                Song_Name = (string?)jObject["trackName"],
                Time_Played = (string?)jObject["msPlayed"]
            }).ToList();

            return spotifyEntries;
        }

        public static string ExportSpotifyJson(List<SpotifyEntry> filteredEntries)
        {
            // Create a list of JSON objects
            List<JObject> json = new();

            foreach (SpotifyEntry entry in filteredEntries)
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["endTime"] = entry.Time_End.ToString(Options.spotifyJsonTime, CultureInfo.InvariantCulture),
                    ["artistName"] = entry.Song_Artist,
                    ["trackName"] = entry.Song_Name,
                    ["msPlayed"] = entry.Time_Played
                };

                json.Add(songEntry);
            }

            // Create a JSON document based on the list of songs within range
            string document = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);

            return document;
        }
    }
}
