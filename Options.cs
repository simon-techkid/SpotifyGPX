// SpotifyGPX by Simon Field

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SpotifyGPX
{
    public class Options
    {
        // Output Time Formats
        public static readonly string gpxPointDescription = "yyyy-MM-dd HH:mm:ssZ"; // time format used in the <desc> field a GPX song point (your choice)
        public static readonly string gpxPointTimeOut = "yyyy-MM-ddTHH:mm:ss.fffZ"; // time format used as the <time> parameter for a GPX song point (ISO 8601)

        // Input Time Formats
        public static readonly string gpxPointTimeInp = "yyyy-MM-ddTHH:mm:ss.fffzzz"; // time format used to interpret GPX track <time> tags
        public static readonly string spotifyJsonTime = "yyyy-MM-dd HH:mm"; // time format of a song entry in Spotify JSON

        public static string Identifier(SpotifyEntry song, string type)
        {
            // Function defining the preferred return strings of GPX point metadata

            // Song Statistics
            string time_end = Spotify.ReadJsonTime(song.endTime).ToString(gpxPointDescription); // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
            string milliseconds_played = song.msPlayed; // This field is the number of milliseconds the stream was played.

            // Track Metadata
            string track_name = song.trackName; // This field is the name of the track.
            string artist = song.artistName; // This field is the name of the artist, band or podcast.

            if (type == "desc")
            {
                // ===================== \\
                // GPX POINT DESCRIPTION \\
                // ===================== \\
                return $"Ended at {time_end}";
            }
            else
            {
                // ============== \\
                // GPX POINT NAME \\
                // ============== \\
                return $"{artist} - {track_name}";
            }
        }

        public static List<SpotifyEntry> ParseSpotifyJson(string inputJson)
        {
            var json = File.ReadAllText(inputJson);
            List<JObject> jObjects = JsonConvert.DeserializeObject<List<JObject>>(json);

            List<SpotifyEntry> spotifyEntries = jObjects.Select(jObject => new SpotifyEntry
            {
                endTime = (string?)jObject["endTime"],
                artistName = (string?)jObject["artistName"],
                trackName = (string?)jObject["trackName"],
                msPlayed = (string?)jObject["msPlayed"]
            }).ToList();

            return spotifyEntries;
        }
    }

    public struct SpotifyEntry
    {
        public string? endTime { get; set; }
        public string? artistName { get; set; }
        public string? trackName { get; set; }
        public string? msPlayed { get; set; }
    }

    public struct GPXPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}