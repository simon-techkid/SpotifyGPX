// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpotifyGPX;

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

    public int Index { get; } // Unique identifier of this song in a list
    public readonly DateTimeOffset Time
    {
        get
        {
            string time = ((string?)track["endTime"] ?? (string?)track["ts"]) ?? throw new Exception($"JSON 'ts' or 'endTime' cannot be null, check your JSON");

            if (DateTimeOffset.TryParseExact(time, Formats.miniSpotFormat, null, DateTimeStyles.AssumeUniversal, out var result))
            {
                return result;
            }
            else if (DateTimeOffset.TryParseExact(time, Formats.fullSpotFormat, null, DateTimeStyles.AssumeUniversal, out result))
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
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(double.Parse(Time_Played));
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
    public override string ToString() => $"{Song_Artist} - {Song_Name}";
}

public readonly struct GPXTrack
{
    public GPXTrack(List<GPXPoint> points, int index)
    {
        Points = points;
        Index = index;
        Start = Points.Select(point => point.Time).Min();
        End = Points.Select(point => point.Time).Max();
    }

    public int Index { get; }

    public List<GPXPoint> Points { get; }

    public readonly DateTimeOffset Start { get; }

    public readonly DateTimeOffset End { get; }

    public override string ToString() => $"[T{Index}] ({Points.Count} points) Starts: {Start}, Ends: {End}";
}

public readonly struct GPXPoint
{
    public GPXPoint(Coordinate point, string time, int index)
    {
        Location = point;
        Time = DateTimeOffset.ParseExact(time, Formats.gpxTimeInp, null);
        Index = index;
    }

    public int Index { get; } // Unique identifier of this point in a list
    public bool Predicted { get; }
    public Coordinate Location { get; }
    public DateTimeOffset Time { get; }
}

public readonly struct Coordinate
{
    public Coordinate(double lat, double lon)
    {
        Latitude = lat;
        Longitude = lon;
    }

    public Coordinate(string lat, string lon)
    {
        Latitude = double.Parse(lat);
        Longitude = double.Parse(lon);
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
    public string GpxDescription()
    {
        // ===================== \\
        // GPX POINT DESCRIPTION \\
        // ===================== \\

        DateTimeOffset EndedAt = Song.Time.ToOffset(Point.Time.Offset);

        StringBuilder builder = new();

        // accuracy < 0 means song ends before the point
        // accuracy == 0 means song ends in the same place as the point
        // accuracy > 0 means song ends after the point
        string seconds = AbsRoundAccuracy == 1 ? "second" : "seconds";
        string ppexpl = $"{(RoundAccuracy < 0 ? $"{AbsRoundAccuracy} {seconds} before, at" : RoundAccuracy == 0 ? "at the same time," : $"{AbsRoundAccuracy} {seconds} after, at")}";

        builder.AppendLine($"At this location at {Point.Time.ToString(Formats.gpxDescriptionPlayedAt)}");
        builder.AppendLine($"Song ended {ppexpl} {EndedAt.ToString(Formats.gpxDescriptionPlayedAt)}");
        builder.AppendLine($"Song played for {Song.TimePlayed.ToString(Formats.gpxDescriptionTimePlayed)}");
        builder.AppendLine($"Skipped: {(Song.Song_Skipped == true ? "Yes" : "No")}");
        builder.AppendLine($"Shuffle: {(Song.Song_Shuffle == true ? "On" : "Off")}");
        builder.AppendLine($"Offline: {(Song.Spotify_Offline == true ? "Yes" : "No")}");
        builder.AppendLine($"IP Address: {Song.Spotify_IP}");
        builder.AppendLine($"Country: {Song.Spotify_Country}");
        builder.Append($"{(Point.Predicted == true ? $"Point Predicted" : null)}");

        return builder.ToString();
    }

    public SongPoint(SpotifyEntry song, GPXPoint point, int index, int trackmem)
    {
        Song = song;
        Point = point;
        Index = index;
        TrackMember = trackmem;
    }

    public readonly int Index { get; } // Unique identifier of this SongPoint in a list
    public readonly int TrackMember { get; } // Track from which the pairing originates
    private readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds;
    public readonly double AbsAccuracy => Math.Abs(Accuracy);
    private readonly double RoundAccuracy => Math.Round(Accuracy);
    private readonly double AbsRoundAccuracy => Math.Abs(RoundAccuracy);
    public SpotifyEntry Song { get; }
    public GPXPoint Point { get; }

    public override string ToString()
    {
        string songTime = Song.Time.ToUniversalTime().ToString(Formats.consoleReadoutFormat);
        string pointTime = Point.Time.ToUniversalTime().ToString(Formats.consoleReadoutFormat);

        return $"[T{TrackMember}#{Point.Index} ==> {Index}] [{songTime} ~ {pointTime}] [{RoundAccuracy}s] {Song}";
    }
}
