// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX.Dependencies
{
    public class SongResponse
    {
        public static string Identifier(SpotifyEntry song, string type)
        {
            // Song Statistics
            string time_end = song.endTime; // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
            string milliseconds_played = song.msPlayed; // This field is the number of milliseconds the stream was played.

            // Track Metadata
            string track_name = song.trackName; // This field is the name of the track.
            string artist = song.artistName; // This field is the name of the artist, band or podcast.

            if (type == "desc")
            {
                // ===================== \\
                // GPX POINT DESCRIPTION \\
                // ===================== \\
                return $"{track_name} by {artist}\nEnded at {time_end}";
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
        public string? endTime { get; set; }
        public string? artistName { get; set; }
        public string? trackName { get; set; }
        public string? msPlayed { get; set; }
    }

    public class GPXPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}