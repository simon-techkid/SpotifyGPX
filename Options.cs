// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Dependencies
{
    public class SongResponse
    {
        public static string Identifier(SpotifyEntry song, string type)
        {
            // Spotify Internals
            string username = song.username; // This field is your Spotify username.
            string platform = song.platform; // This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
            string origin_country = song.conn_country; // This field is the country code of the country where the stream was played (e.g. SE - Sweden).
            string ip_address = song.ip_addr_decrypted; // This field contains the IP address logged when streaming the track.
            string user_agent = song.user_agent_decrypted; // This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari)
            string offline = $"{(song.offline == "true" ? "Yes" : "No")}"; // This field indicates whether the track was played in offline mode (“True”) or not (“False”).
            string offline_timestamp = song.offline_timestamp; // This field is a timestamp of when offline mode was used, if used.
            string incognito = $"{(song.incognito == "true" ? "Enabled" : "Disabled")}"; // This field indicates whether the track was played in incognito mode (“True”) or not (“False”).

            // Song Statistics
            string time_end = song.ts; // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
            string milliseconds_played = song.ms_played; // This field is the number of milliseconds the stream was played.
            string reason_start = song.reason_start; // This field is a value telling why the track started (e.g. “trackdone”)
            string reason_end = song.reason_end; // This field is a value telling why the track ended (e.g. “endplay”).
            string shuffle = $"{(song.shuffle == "true" ? "On" : "Off")}"; // This field has the value True or False depending on if shuffle mode was used when playing the track.
            string skipped = $"{(song.skipped == "true" ? "Yes" : "No")}"; // This field indicates if the user skipped to the next song

            // Track Metadata
            string track_name = song.master_metadata_track_name; // This field is the name of the track.
            string artist = song.master_metadata_album_artist_name; // This field is the name of the artist, band or podcast.
            string album = song.master_metadata_album_album_name; // This field is the name of the album of the track.
            string song_url = song.spotify_track_uri; // A Spotify URI, uniquely identifying the track in the form of “spotify:track:<base-62 string>”. A Spotify URI is a resource identifier that you can enter, for example, in the Spotify Desktop client’s search box to locate an artist, album, or track.

            // Episode Metadata
            string episode_name = song.episode_name; // This field contains the name of the episode of the podcast.
            string show_name = song.episode_show_name; // This field contains the name of the show of the podcast.
            string episode_url = song.spotify_episode_uri; // A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:<base-62 string>”. A Spotify Episode URI is a resource identifier that you can enter, for example, in the Spotify Desktop client’s search box to locate an episode of a podcast.

            if (type == "desc")
            {
                // ===================== \\
                // GPX POINT DESCRIPTION \\
                // ===================== \\
                return $"{track_name} by {artist}\nEnded: {time_end}\nShuffle: {shuffle} | Skipped: {skipped} | Offline: {offline}\nIP Address: {ip_address} | Country: {origin_country}";
            }
            else
            {
                // ============== \\
                // GPX POINT NAME \\
                // ============== \\
                return $"{artist} - {track_name}";
            }
        }
    }

    public class SpotifyEntry
    {
        public string? ts { get; set; }
        public string? username { get; set; }
        public string? platform { get; set; }
        public string? ms_played { get; set; }
        public string? conn_country { get; set; }
        public string? ip_addr_decrypted { get; set; }
        public string? user_agent_decrypted { get; set; }
        public string? master_metadata_track_name { get; set; }
        public string? master_metadata_album_artist_name { get; set; }
        public string? master_metadata_album_album_name { get; set; }
        public string? spotify_track_uri { get; set; }
        public string? episode_name { get; set; }
        public string? episode_show_name { get; set; }
        public string? spotify_episode_uri { get; set; }
        public string? reason_start { get; set; }
        public string? reason_end { get; set; }
        public string? shuffle { get; set; }
        public string? skipped { get; set; }
        public string? offline { get; set; }
        public string? offline_timestamp { get; set; }
        public string? incognito { get; set; }
    }

    public class GPXPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}