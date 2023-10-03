// SpotifyGPX by Simon Field

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

            // Spotify Backend
            string Username = song.Spotify_Username; // This field is your Spotify username.
            string Platform = song.Spotify_Platform; // This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
            string Origin_Country = song.Spotify_Country; // This field is the country code of the country where the stream was played (e.g. SE - Sweden).
            string IP_Address = song.Spotify_IP; // This field contains the IP address logged when streaming the track.
            string User_Agent = song.Spotify_UA; // This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari)
            string Offline = $"{(song.Spotify_Offline == "true" ? "Yes" : "No")}"; // This field indicates whether the track was played in offline mode (“True”) or not (“False”).
            string Offline_Timestamp = song.Spotify_OfflineTS; // This field is a timestamp of when offline mode was used, if used.
            string Incognito = $"{(song.Spotify_Incognito == "true" ? "Enabled" : "Disabled")}"; // This field indicates whether the track was played in incognito mode (“True”) or not (“False”).

            // Track Metadata
            string Title = song.Song_Name; // This field is the name of the track.
            string Artist = song.Song_Artist; // This field is the name of the artist, band or podcast.
            string Album = song.Song_Album;
            string URL = song.Song_URI;
            string StartReason = song.Song_StartReason;
            string EndReason = song.Song_EndReason;
            string Shuffled = $"{(song.Song_Shuffle == "true" ? "On" : "Off")}"; // This field has the value True or False depending on if shuffle mode was used when playing the track.
            string Skipped = $"{(song.Song_Skipped == "true" ? "Yes" : "No")}"; // This field indicates if the user skipped to the next song

            // Episode Metadata
            string Episode_Title = song.Episode_Name;
            string Episode_Show = song.Episode_Show;
            string Episode_URL = song.Episode_URI;

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