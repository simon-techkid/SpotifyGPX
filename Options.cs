// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;

namespace SpotifyGPX
{
    public class Options
    {
        // Output Time Formats
        public static readonly string gpxPointDescription = "HH:mm:ss"; // time format used in the <desc> field a GPX song point (your choice)

        // Input Time Formats
        public static readonly string gpxPointTimeInp = "yyyy-MM-ddTHH:mm:ss.fffzzz"; // time format used to interpret GPX track <time> tags

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
                return $"Ended here, at {EndedAt}";
            }
            else
            {
                // ============== \\
                // GPX POINT NAME \\
                // ============== \\
                return $"{Artist} - {Title}";
            }
        }
    }

    public struct SpotifyEntry
    {
        public DateTimeOffset Time_End { get; set; }
        public string? Song_Artist { get; set; }
        public string? Song_Name { get; set; }
        public string? Time_Played { get; set; }
        public string? Spotify_Username { get; set; }
        public string? Spotify_Platform { get; set; }
        public string? Spotify_Country { get; set; }
        public string? Spotify_IP { get; set; }
        public string? Spotify_UA { get; set; }
        public string? Song_Album { get; set; }
        public string? Song_URI { get; set; }
        public string? Episode_Name { get; set; }
        public string? Episode_Show { get; set; }
        public string? Episode_URI { get; set; }
        public string? Song_StartReason { get; set; }
        public string? Song_EndReason { get; set; }
        public string? Song_Shuffle { get; set; }
        public string? Song_Skipped { get; set; }
        public string? Spotify_Offline { get; set; }
        public string? Spotify_OfflineTS { get; set; }
        public string? Spotify_Incognito { get; set; }
    }

    public struct GPXPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}