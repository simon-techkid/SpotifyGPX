// SpotifyGPX by Simon Field

using System;

#nullable enable

namespace SpotifyGPX
{
    public class Options
    {
        // Time format as read from the input GPX file:
        public static readonly string gpxPointTimeInp = "yyyy-MM-ddTHH:mm:ss.fffzzz"; // time format used to interpret GPX track <time> tags

        // Time format of the Spotify track portrayed in the GPX point description:
        public static readonly string gpxPointDescription = "yyyy-MM-dd HH:mm:ss zzz"; // time format used in the <desc> field a GPX song point (your choice)

        // Time format for console readout of point-song time comparison:
        public static readonly string consoleReadoutFormat = "HH:mm:ss";

        public static string GpxTitle(SpotifyEntry song)
        {
            // ============== \\
            // GPX POINT NAME \\
            // ============== \\

            return $"{song.Song_Artist} - {song.Song_Name}";
        }

        public static string GpxDescription(SpotifyEntry song, TimeSpan offset, string? message)
        {
            // Function defining the preferred return strings of GPX point metadata

            // Spotify Backend
            string? Username = song.Spotify_Username; // This field is your Spotify username.
            string? Platform = song.Spotify_Platform; // This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
            string? Origin_Country = song.Spotify_Country; // This field is the country code of the country where the stream was played (e.g. SE - Sweden).
            string? IP_Address = song.Spotify_IP; // This field contains the IP address logged when streaming the track.
            string? User_Agent = song.Spotify_UA; // This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari)
            bool? Offline = song.Spotify_Offline; // This field indicates whether the track was played in offline mode (“True”) or not (“False”).
            string? Offline_Timestamp = song.Spotify_OfflineTS; // This field is a timestamp of when offline mode was used, if used.
            bool? Incognito = song.Spotify_Incognito; // This field indicates whether the track was played in incognito mode (“True”) or not (“False”).

            // Track Metadata
            string? Title = song.Song_Name; // This field is the name of the track.
            string? Artist = song.Song_Artist; // This field is the name of the artist, band or podcast.
            string? Album = song.Song_Album; // This field is the name of the album of the track.
            string? URL = song.Song_URI; // A Spotify URI, uniquely identifying the track in the form of “spotify:track:<base-62 string>”
            string? StartReason = song.Song_StartReason; // This field is a value telling why the track started (e.g. “trackdone”)
            string? EndReason = song.Song_EndReason; // This field is a value telling why the track ended (e.g. “endplay”).
            bool? Shuffled = song.Song_Shuffle; // This field has the value True or False depending on if shuffle mode was used when playing the track.
            bool? Skipped = song.Song_Skipped; // This field indicates if the user skipped to the next song

            // Episode Metadata
            string? Episode_Title = song.Episode_Name; // This field contains the name of the episode of the podcast.
            string? Episode_Show = song.Episode_Show; // This field contains the name of the show of the podcast.
            string? Episode_URL = song.Episode_URI; // A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:<base-62 string>”

            // Duration Information
            DateTimeOffset EndedAt = new(song.Time_End.Ticks + offset.Ticks, offset); // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
            string? PlayedMs = song.Time_Played; // This field is the number of milliseconds the stream was played.

            string returnString = "";

            // ===================== \\
            // GPX POINT DESCRIPTION \\
            // ===================== \\

            string description = "";

            description += $"{(song.Time_End != null ? $"Ended here, at {EndedAt.ToString(gpxPointDescription)}" : null)}";
            description += $"{(song.Song_Shuffle != null ? $"\nShuffle: {(Shuffled == true ? "On" : "Off")}" : null)}";
            description += $"{(song.Song_Skipped != null ? $"\nSkipped: {(Skipped == true ? "Yes" : "No")}" : null)}";
            description += $"{(song.Spotify_Offline != null ? $"\nOffline: {(Offline == true ? "Yes" : "No")}" : null)}";
            description += $"{(song.Spotify_IP != null ? $"\nIP Address: {IP_Address}" : null)}";
            description += $"{(song.Spotify_Country != null ? $"\nCountry: {Origin_Country}" : null)}";
            description += $"{(message != null ? $"\n{message}" : null)}";

            returnString += description;

            return returnString;
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
        public bool? Song_Shuffle { get; set; }
        public bool? Song_Skipped { get; set; }
        public bool? Spotify_Offline { get; set; }
        public string? Spotify_OfflineTS { get; set; }
        public bool? Spotify_Incognito { get; set; }
    }

    public struct GPXPoint
    {
        public bool? Predicted { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}