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
            string EndedAt = Spotify.ReadJsonTime(song.Time_End).ToString(gpxPointDescription); // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
            string PlayedMs = song.Time_Played; // This field is the number of milliseconds the stream was played.

            if (type == "desc")
            {
                // ===================== \\
                // GPX POINT DESCRIPTION \\
                // ===================== \\
                return $"Ended: {EndedAt}\nShuffle: {Shuffled} | Skipped: {Skipped} | Offline: {Offline}\nIP Address: {IP_Address} | Country: {Origin_Country}";
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
            var json = File.ReadAllText(inputJson);
            List<JObject> jObjects = JsonConvert.DeserializeObject<List<JObject>>(json);

            List<SpotifyEntry> spotifyEntries = jObjects.Select(jObject => new SpotifyEntry
            {
                Time_End = (string?)jObject["ts"],
                Spotify_Username = (string?)jObject["username"],
                Spotify_Platform = (string?)jObject["platform"],
                Time_Played = (string?)jObject["ms_played"],
                Spotify_Country = (string?)jObject["conn_country"],
                Spotify_IP = (string?)jObject["ip_addr_decrypted"],
                Spotify_UA = (string?)jObject["user_agent_decrypted"],
                Song_Name = (string?)jObject["master_metadata_track_name"],
                Song_Artist = (string?)jObject["master_metadata_album_artist_name"],
                Song_Album = (string?)jObject["master_metadata_album_album_name"],
                Song_URI = (string?)jObject["spotify_track_uri"],
                Episode_Name = (string?)jObject["episode_name"],
                Episode_Show = (string?)jObject["episode_show_name"],
                Episode_URI = (string?)jObject["spotify_episode_uri"],
                Song_StartReason = (string?)jObject["reason_start"],
                Song_EndReason = (string?)jObject["reason_end"],
                Song_Shuffle = (string?)jObject["shuffle"],
                Song_Skipped = (string?)jObject["skipped"],
                Spotify_Offline = (string?)jObject["offline"],
                Spotify_OfflineTS = (string?)jObject["offline_timestamp"],
                Spotify_Incognito = (string?)jObject["incognito"]
            }).ToList();

            return spotifyEntries;
        }
    }

    public struct SpotifyEntry
    {
        [JsonProperty(PropertyName = "ts")]
        public string? Time_End { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string? Spotify_Username { get; set; }
        [JsonProperty(PropertyName = "platform")]
        public string? Spotify_Platform { get; set; }
        [JsonProperty(PropertyName = "ms_played")]
        public string? Time_Played { get; set; }
        [JsonProperty(PropertyName = "conn_country")]
        public string? Spotify_Country { get; set; }
        [JsonProperty(PropertyName = "ip_addr_decrypted")]
        public string? Spotify_IP { get; set; }
        [JsonProperty(PropertyName = "user_agent_decrypted")]
        public string? Spotify_UA { get; set; }
        [JsonProperty(PropertyName = "master_metadata_track_name")]
        public string? Song_Name { get; set; }
        [JsonProperty(PropertyName = "master_metadata_album_artist_name")]
        public string? Song_Artist { get; set; }
        [JsonProperty(PropertyName = "master_metadata_album_album_name")]
        public string? Song_Album { get; set; }
        [JsonProperty(PropertyName = "spotify_track_uri")]
        public string? Song_URI { get; set; }
        [JsonProperty(PropertyName = "episode_name")]
        public string? Episode_Name { get; set; }
        [JsonProperty(PropertyName = "episode_show_name")]
        public string? Episode_Show { get; set; }
        [JsonProperty(PropertyName = "spotify_episode_uri")]
        public string? Episode_URI { get; set; }
        [JsonProperty(PropertyName = "reason_start")]
        public string? Song_StartReason { get; set; }
        [JsonProperty(PropertyName = "reason_end")]
        public string? Song_EndReason { get; set; }
        [JsonProperty(PropertyName = "shuffle")]
        public string? Song_Shuffle { get; set; }
        [JsonProperty(PropertyName = "skipped")]
        public string? Song_Skipped { get; set; }
        [JsonProperty(PropertyName = "offline")]
        public string? Spotify_Offline { get; set; }
        [JsonProperty(PropertyName = "offline_timestamp")]
        public string? Spotify_OfflineTS { get; set; }
        [JsonProperty(PropertyName = "incognito")]
        public string? Spotify_Incognito { get; set; }
    }

    public struct GPXPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}