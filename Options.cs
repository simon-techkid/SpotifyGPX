// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace SpotifyGPX
{
    public class Options
    {
        // Output Time Formats
        public static readonly string gpxPointDescription = "yyyy-MM-dd HH:mm:ss.fffzzz"; // time format used in the <desc> field a GPX song point (your choice)

        // Input Time Formats
        public static readonly string gpxPointTimeInp = "yyyy-MM-ddTHH:mm:ss.fffzzz"; // time format used to interpret GPX track <time> tags
        public static readonly string spotifyJsonTime = "yyyy-MM-dd HH:mm"; // time format of a song entry in Spotify JSON

        public static string Identifier(SpotifyEntry song, TimeSpan offset, string type)
        {
            // Function defining the preferred return strings of GPX point metadata

            // Track Metadata
            string Title = song.Song_Name; // This field is the name of the track.
            string Artist = song.Song_Artist; // This field is the name of the artist, band or podcast.

            // Duration Information
            string EndedAt = song.Time_End.Add(offset).ToString(gpxPointDescription); // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
            string PlayedMs = song.Time_Played; // This field is the number of milliseconds the stream was played.

            if (type == "desc")
            {
                // ===================== \\
                // GPX POINT DESCRIPTION \\
                // ===================== \\
                return $"Ended at {EndedAt}";
            }
            else
            {
                // ============== \\
                // GPX POINT NAME \\
                // ============== \\
                return $"{Artist} - {Title}";
            }
        }

        public static List<SpotifyEntry> ParseSpotifyJson(string inputJson)
        {
            List<JObject> jObjects = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(inputJson));

            List<SpotifyEntry> spotifyEntries = jObjects.Select(jObject => new SpotifyEntry
            {
                Time_End = DateTimeOffset.ParseExact((string?)jObject["endTime"], spotifyJsonTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                Song_Artist = (string?)jObject["artistName"],
                Song_Name = (string?)jObject["trackName"],
                Time_Played = (string?)jObject["msPlayed"]
            }).ToList();

            return spotifyEntries;
        }
    }

    public struct SpotifyEntry
    {
        [JsonProperty(PropertyName = "endTime")]
        public DateTimeOffset Time_End { get; set; }
        [JsonProperty(PropertyName = "artistName")]
        public string? Song_Artist { get; set; }
        [JsonProperty(PropertyName = "trackName")]
        public string? Song_Name { get; set; }
        [JsonProperty(PropertyName = "msPlayed")]
        public string? Time_Played { get; set; }
    }

    public struct GPXPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}