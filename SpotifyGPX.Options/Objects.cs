// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text;

#nullable enable

namespace SpotifyGPX.Options;

public readonly struct SpotifyEntry
{
    public SpotifyEntry(JObject json, int index)
    {
        try
        {
            Index = index;
            track = json;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing contents of JSON tag:\n{json} to a valid song entry:\n{ex}");
        }
    }

    private readonly JObject track;

    public int Index { get; }
    public readonly DateTimeOffset Time
    {
        get
        {
            string time = (string?)track["endTime"] ?? (string?)track["ts"];

            if (DateTimeOffset.TryParseExact(time, Point.miniSpotFormat, null, DateTimeStyles.AssumeUniversal, out var result))
            {
                return result;
            }
            else if (DateTimeOffset.TryParseExact(time, Point.fullSpotFormat, null, DateTimeStyles.AssumeUniversal, out result))
            {
                return result;
            }
            else
            {
                throw new Exception(""); // provide later
            }
        }
    }

    public readonly string? Song_Artist => (string?)track["artistName"] ?? (string?)track["master_metadata_album_artist_name"];
    public readonly string? Song_Name => (string?)track["trackName"] ?? (string?)track["master_metadata_track_name"];
    public readonly string? Time_Played => (string?)track["msPlayed"] ?? (string?)track["ms_played"];
    public readonly string? Spotify_Username => (string?)track["username"];
    public readonly string? Spotify_Platform => (string?)track["platform"];
    public readonly string? Spotify_Country => (string?)track["conn_country"];
    public readonly string? Spotify_IP => (string?)track["ip_addr_decrypted"];
    public readonly string? Spotify_UA => (string?)track["user_agent_decrypted"];
    public readonly string? Song_Album => (string?)track["master_metadata_album_album_name"];
    public readonly string? Song_URI => (string?)track["spotify_track_uri"];
    public readonly string? Episode_Name => (string?)track["episode_name"];
    public readonly string? Episode_Show => (string?)track["episode_show_name"];
    public readonly string? Episode_URI => (string?)track["spotify_episode_uri"];
    public readonly string? Song_StartReason => (string?)track["reason_start"];
    public readonly string? Song_EndReason => (string?)track["reason_end"];
    public readonly bool? Song_Shuffle => (bool?)track["shuffle"];
    public readonly bool? Song_Skipped => (bool?)track["skipped"];
    public readonly bool? Spotify_Offline => (bool?)track["offline"];
    public readonly string? Spotify_OfflineTS => (string?)track["offline_timestamp"];
    public readonly bool? Spotify_Incognito => (bool?)track["incognito"];
}

public readonly struct GPXPoint
{
    public GPXPoint(Coordinate point, string time, int trackmem, int index)
    {
        Location = point;
        Time = DateTimeOffset.ParseExact(time, Point.gpxTimeInp, null);
        TrackMember = trackmem;
        Index = index;
    }

    public int Index { get; }
    public bool Predicted { get; }
    public Coordinate Location { get; }
    public DateTimeOffset Time { get; }
    public int TrackMember { get; }
}

public readonly struct Coordinate
{
    public Coordinate(double lat, double lon)
    {
        Latitude = lat;
        Longitude = lon;
    }

    public readonly double Latitude;
    public readonly double Longitude;

    public override bool Equals(object? obj)
    {
        if (obj is Coordinate other)
        {
            return this.Latitude == other.Latitude && this.Longitude == other.Longitude;
        }
        return false;
    }

    public static double operator -(Coordinate coord1, Coordinate coord2)
    {
        // Calculate distance between coord1 and coord2
        double latDiff = coord2.Latitude - coord1.Latitude;
        double lonDiff = coord2.Longitude - coord1.Longitude;

        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        return distance;
    }

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public static bool operator ==(Coordinate a, Coordinate b) => a.Equals(b);

    public static bool operator !=(Coordinate a, Coordinate b) => !a.Equals(b);

    public override string ToString() => $"{(Latitude, Longitude)}";

    public (double, double) ToTuple() => (Latitude, Longitude);
}

public readonly struct SongPoint
{
    public string GpxTitle()
    {
        // ============== \\
        // GPX POINT NAME \\
        // ============== \\

        return $"{Song.Song_Artist} - {Song.Song_Name}";
    }

    public string GpxDescription()
    {
        // ===================== \\
        // GPX POINT DESCRIPTION \\
        // ===================== \\

        DateTimeOffset EndedAt = new(Song.Time.Ticks + Point.Time.Offset.Ticks, Point.Time.Offset);

        StringBuilder builder = new();

        builder.AppendLine($"Ended here, at {EndedAt.ToString(Options.Point.gpxPointDescription)}");
        builder.AppendLine($"Song is {AbsAccuracy} seconds {(Accuracy < 0 ? "behind the" : "ahead of the")} point");
        builder.AppendLine($"{(Song.Song_Shuffle != null ? $"Shuffle: {(Song.Song_Shuffle == true ? "On" : "Off")}" : null)}");
        builder.AppendLine($"{(Song.Song_Skipped != null ? $"Skipped: {(Song.Song_Skipped == true ? "Yes" : "No")}" : null)}");
        builder.AppendLine($"{(Song.Spotify_Offline != null ? $"Offline: {(Song.Spotify_Offline == true ? "Yes" : "No")}" : null)}");
        builder.AppendLine($"{(Song.Spotify_IP != null ? $"IP Address: {Song.Spotify_IP}" : null)}");
        builder.AppendLine($"{(Song.Spotify_Country != null ? $"Country: {Song.Spotify_Country}" : null)}");
        builder.Append($"{(Point.Predicted == true ? $"Point Predicted" : null)}");

        return builder.ToString();
    }

    public SongPoint(SpotifyEntry song, GPXPoint point, int index)
    {
        Song = song;
        Point = point;
        Index = index;
    }

    public readonly int Index { get; } // Unique identifier of this SongPoint in a list
    public readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds;
    public readonly double AbsAccuracy => Math.Abs(Accuracy);
    public SpotifyEntry Song { get; }
    public GPXPoint Point { get; }

    public override string ToString()
    {
        string songTime = Song.Time.ToUniversalTime().ToString(Options.Point.consoleReadoutFormat);
        string pointTime = Point.Time.ToUniversalTime().ToString(Options.Point.consoleReadoutFormat);

        return $"[CORR] [T{Point.TrackMember}] [{Index + 1}] [{Song.Index}] [{Point.Index}] [{songTime} ~ {pointTime}] [{Math.Round(Accuracy)}s] {GpxTitle()}";
    }
}
