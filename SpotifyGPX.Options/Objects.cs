// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text;

#nullable enable

namespace SpotifyGPX.Options;

public struct SpotifyEntry
{
    public SpotifyEntry(JObject track, int index)
    {
        try
        {
            Index = index;
            TimeStr = (string?)track["endTime"] ?? (string?)track["ts"];
            Spotify_Username = (string?)track["username"];
            Spotify_Platform = (string?)track["platform"];
            Time_Played = (string?)track["msPlayed"] ?? (string?)track["ms_played"];
            Spotify_Country = (string?)track["conn_country"];
            Spotify_IP = (string?)track["ip_addr_decrypted"];
            Spotify_UA = (string?)track["user_agent_decrypted"];
            Song_Name = (string?)track["trackName"] ?? (string?)track["master_metadata_track_name"];
            Song_Artist = (string?)track["artistName"] ?? (string?)track["master_metadata_album_artist_name"];
            Song_Album = (string?)track["master_metadata_album_album_name"];
            Song_URI = (string?)track["spotify_track_uri"];
            Episode_Name = (string?)track["episode_name"];
            Episode_Show = (string?)track["episode_show_name"];
            Episode_URI = (string?)track["spotify_episode_uri"];
            Song_StartReason = (string?)track["reason_start"];
            Song_EndReason = (string?)track["reason_end"];
            Song_Shuffle = (bool?)track["shuffle"];
            Song_Skipped = (bool?)track["skipped"];
            Spotify_Offline = (bool?)track["offline"];
            Spotify_OfflineTS = (string?)track["offline_timestamp"];
            Spotify_Incognito = (bool?)track["incognito"];
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing contents of JSON tag:\n{track} to a valid song entry:\n{ex}");
        }
    }

    public int Index { get; }
    public DateTimeOffset Time { get; private set; }
    public string TimeStr
    {
        readonly get
        {
            return Time.ToString(Point.outJsonFormat, CultureInfo.InvariantCulture);
        }
        set
        {
            if (DateTimeOffset.TryParseExact(value, Point.miniSpotFormat, null, DateTimeStyles.AssumeUniversal, out var result))
            {
                Time = result;
            }
            else if (DateTimeOffset.TryParseExact(value, Point.fullSpotFormat, null, DateTimeStyles.AssumeUniversal, out result))
            {
                Time = result;
            }
            else
            {
                throw new Exception(""); // provide later
            }
        }
    }

    public enum ReturnTag
    {
        Creator,
        Title,
        Annotation,
        Duration
    }

    public readonly string Tag(ReturnTag tag)
    {
        switch (tag)
        {
            case ReturnTag.Creator:
                return Song_Artist;
            case ReturnTag.Title:
                return Song_Name;
            case ReturnTag.Annotation:
                return Time.ToString(Point.gpxTimeOut);
            case ReturnTag.Duration:
                return Time_Played;
            default:
                return null; // Handle other cases if needed
        }
    }

    public string? Song_Artist { get; }
    public string? Song_Name { get; }
    public string? Time_Played { get; }
    public string? Spotify_Username { get; }
    public string? Spotify_Platform { get; }
    public string? Spotify_Country { get; }
    public string? Spotify_IP { get; }
    public string? Spotify_UA { get; }
    public string? Song_Album { get; }
    public string? Song_URI { get; }
    public string? Episode_Name { get; }
    public string? Episode_Show { get; }
    public string? Episode_URI { get; }
    public string? Song_StartReason { get; }
    public string? Song_EndReason { get; }
    public bool? Song_Shuffle { get; }
    public bool? Song_Skipped { get; }
    public bool? Spotify_Offline { get; }
    public string? Spotify_OfflineTS { get; }
    public bool? Spotify_Incognito { get; }
}

public struct GPXPoint
{
    public GPXPoint(Coordinate point, string time, int trackmem, int index)
    {
        Location = point;
        TimeStr = time;
        TrackMember = trackmem;
        Index = index;
    }

    public GPXPoint(Coordinate point)
    {
        Location = point;
    }

    public GPXPoint PPModify(Coordinate point)
    {
        Location = point;
        Predicted = true;

        return this;
    }

    public int Index { get; }
    public bool Predicted { get; private set; }
    public Coordinate Location { get; private set; }
    public DateTimeOffset Time { get; private set; }
    public string TimeStr
    {
        readonly get => Time.ToString(Point.gpxTimeOut);
        private set => Time = DateTimeOffset.ParseExact(value, Point.gpxTimeInp, null);
    }
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

        double lat1 = coord1.Latitude;
        double lon1 = coord1.Longitude;
        double lat2 = coord2.Latitude;
        double lon2 = coord2.Longitude;

        double latDiff = lat2 - lat1;
        double lonDiff = lon2 - lon1;

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
        builder.AppendLine($"Song is {Math.Abs(Accuracy)} seconds {(Accuracy < 0 ? "behind the" : "ahead of the")} point");
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
    public readonly double AbsAccuracy => Math.Abs((Point.Time - Song.Time).TotalSeconds);
    public SpotifyEntry Song { get; }
    public GPXPoint Point { get; }

    public override string ToString()
    {
        string songTime = Song.Time.ToUniversalTime().ToString(Options.Point.consoleReadoutFormat);
        string pointTime = Point.Time.ToUniversalTime().ToString(Options.Point.consoleReadoutFormat);


        return $"[CORR] [T{Point.TrackMember}] [{Index}] [{songTime} ~ {pointTime}] [{Math.Round(Accuracy)}s] {GpxTitle()}";
    }
}
