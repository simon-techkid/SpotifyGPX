// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

public readonly struct SpotifyEntry
{
    public SpotifyEntry(int index, JObject json)
    {
        try
        {
            Index = index;
            Json = json;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing contents of JSON tag:\n{json}\nto a valid song entry:\n{ex}");
        }
    }

    public readonly JObject Json { get; } // Original Json object for this song

    public readonly int Index { get; } // Unique identifier of this SpotifyEntry in a list
    public readonly DateTimeOffset Time
    {
        get
        {
            string time = ((string?)Json["endTime"] ?? (string?)Json["ts"]) ?? throw new Exception($"JSON 'ts' or 'endTime' cannot be null, check your JSON");

            if (DateTimeOffset.TryParseExact(time, Options.SpotifyMini, null, Options.SpotifyTimeStyle, out var result))
            {
                return result; // return parsed "account data" format song end time
            }
            else if (DateTimeOffset.TryParseExact(time, Options.SpotifyFull, null, Options.SpotifyTimeStyle, out result))
            {
                return result; // return parsed "extended streaming history" format song end time
            }
            else
            {
                throw new Exception("Couldn't find valid time formats, 'ts' or 'endTime' in JSON");
            }
        }
    }
    public readonly string? Song_Artist => (string?)Json["artistName"] ?? (string?)Json["master_metadata_album_artist_name"];
    public readonly string? Song_Name => (string?)Json["trackName"] ?? (string?)Json["master_metadata_track_name"];
    private readonly string? Time_Played => (string?)Json["msPlayed"] ?? (string?)Json["ms_played"];
    public readonly TimeSpan? TimePlayed
    {
        get
        {
            if (Time_Played != null)
            {
                return TimeSpan.FromMilliseconds(double.Parse(Time_Played)); // Parse string of milliseconds to TimeSpan
            }
            else
            {
                return null;
            }
        }
    }
    public readonly string? Spotify_Username => (string?)Json["username"];
    public readonly string? Spotify_Platform => (string?)Json["platform"];
    public readonly string? Spotify_Country => (string?)Json["conn_country"];
    public readonly string? Spotify_IP => (string?)Json["ip_addr_decrypted"];
    public readonly string? Spotify_UA => (string?)Json["user_agent_decrypted"];
    public readonly string? Song_Album => (string?)Json["master_metadata_album_album_name"];
    public readonly string? Song_URI => (string?)Json["spotify_track_uri"];
    public readonly string? Episode_Name => (string?)Json["episode_name"];
    public readonly string? Episode_Show => (string?)Json["episode_show_name"];
    public readonly string? Episode_URI => (string?)Json["spotify_episode_uri"];
    public readonly string? Song_StartReason => (string?)Json["reason_start"];
    public readonly string? Song_EndReason => (string?)Json["reason_end"];
    public readonly bool? Song_Shuffle => (bool?)Json["shuffle"];
    public readonly bool? Song_Skipped => (bool?)Json["skipped"];
    public readonly bool? Spotify_Offline => (bool?)Json["offline"];
    private readonly string? Spotify_OfflineTS => (string?)Json["offline_timestamp"];
    public readonly DateTimeOffset? OfflineTimestamp
    {
        get
        {
            if (Spotify_OfflineTS != null)
            {
                return DateTimeOffset.FromUnixTimeSeconds(long.Parse(Spotify_OfflineTS));
            }
            else
            {
                return null;
            }
        }
    }
    public readonly bool? Spotify_Incognito => (bool?)Json["incognito"];
    public override string ToString() => $"{Song_Artist} - {Song_Name}"; // Display format for this song
    public bool WithinTimeFrame(DateTimeOffset Start, DateTimeOffset End) => (Time >= Start) && (Time <= End); // Return true if song within provided time range
}

public readonly struct GPXTrack
{
    public GPXTrack(int? index, string? name, TrackType type, List<GPXPoint> points)
    {
        Track = new TrackInfo(index, name, type);
        Points = points;

        Start = Points.Select(point => point.Time).Min(); // Earliest point's time
        End = Points.Select(point => point.Time).Max(); // Latest point's time

        // Either above or below start/end parsing works, your choice

        // Start = Points.Select(point => point.Time).First(); // First point's time
        // End = Points.Select(point => point.Time).Last(); // Last point's time
    }

    public readonly TrackInfo Track { get; } // Metadata for this track, including its name and index in a list
    public readonly List<GPXPoint> Points { get; } // Where and when were all the points in this track taken?
    public readonly DateTimeOffset Start { get; } // What time was the earliest point logged?
    public readonly DateTimeOffset End { get; } // What time was the latest point logged?
    public override string ToString() // Display format for this track
    {
        StringBuilder builder = new();

        builder.Append("   Name: {0}", Track.ToString());
        builder.Append("   Points: {0}", Points.Count);
        builder.Append("   Starts: {0}", Start.ToString(Options.ConsoleTrack));
        builder.Append("   Ends: {0}", End.ToString(Options.ConsoleTrack));
        builder.Append("   Type: {0}", Track.Type);

        return builder.ToString();
    }
}

public readonly struct TrackInfo
{
    public TrackInfo(int? index, string? name, TrackType type)
    {
        Indexx = index;
        NodeName = name;
        Type = type;
    }

    private readonly int? Indexx { get; }
    public readonly int Index => Indexx == null ? (int)Type : (int)Indexx; // If provided index null, use index of TrackType
    private readonly string? NodeName { get; }
    public readonly string Name => NodeName ?? $"T{Index}";
    public TrackType Type { get; }
    public override string ToString() => Name;
}

public enum TrackType
{
    GPX, // Created from a user provided GPX file track
    Gap, // Created from a gap between GPX tracks
    Combined // Created from all GPX points combined (regardless of track)
}

public readonly struct GPXPoint
{
    public GPXPoint(int index, Coordinate point, string time)
    {
        Index = index;
        Location = point;
        Time = DateTimeOffset.ParseExact(time, Options.GpxInput, null, Options.GpxTimeStyle);
    }

    public readonly int Index { get; } // Unique identifier of this GPXPoint in a list
    public readonly Coordinate Location { get; } // Where on Earth is this point?
    public readonly DateTimeOffset Time { get; } // When was the user at this point?
    // GPXPoint never printed so no need to provide display format
}

public readonly struct Coordinate
{
    public Coordinate(double lat, double lon)
    {
        Latitude = lat;
        Longitude = lon;
    }

    public readonly double Latitude { get; }
    public readonly double Longitude { get; }
    // Coordinate never printed so no need to provide display format
}

public readonly struct SongPoint
{
    public readonly string Description // GPX point description in exported GPX
    {
        get
        {
            StringBuilder builder = new();

            builder.Append("At this position: {0}", PointTime.ToString(Options.DescriptionPlayedAt));
            builder.Append("Song ended: {0}", SongTime.ToString(Options.DescriptionPlayedAt));
            builder.Append("Played for {0}", Song.TimePlayed);
            builder.Append("Skipped: {0}", Song.Song_Skipped);
            builder.Append("Shuffle: {0}", Song.Song_Shuffle);
            builder.Append("IP Address: {0}", Song.Spotify_IP);
            builder.Append("Country: {0}", Song.Spotify_Country);

            return builder.ToString();
        }
    }

    public SongPoint(int index, SpotifyEntry song, GPXPoint point, TrackInfo origin)
    {
        Index = index;
        Song = song;
        Point = point;
        Origin = origin;
    }

    public readonly int Index { get; } // Unique identifier of this SongPoint in a list
    public readonly SpotifyEntry Song { get; } // Contents of the original song entry
    public readonly GPXPoint Point { get; } // Contents of the original GPX point
    public readonly TrackInfo Origin { get; } // Track from which the pairing was created
    public readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds; // Raw accuracy
    public readonly double AbsAccuracy => Math.Abs(Accuracy); // Absolute value of the accuracy
    private readonly double RoundAccuracy => Math.Round(Accuracy); // Rounded accuracy
    public readonly TimeSpan NormalizedOffset => Point.Time.Offset; // Standard offset is defined by the original GPX point offset
    public DateTimeOffset SongTime => Song.Time.ToOffset(NormalizedOffset); // Song end time, normalized to point time zone
    public DateTimeOffset PointTime => Point.Time.ToOffset(NormalizedOffset); // Point end time, normalized to point time zone (redundant)

    public override string ToString()
    {
        // Set both the song and point times to the UTC offset provided by the original GPX point
        string songTime = SongTime.ToString(Options.Console);
        string pointTime = PointTime.ToString(Options.Console);

        // Print information about the pairing
        return $"[{Origin.ToString()}] [P{Point.Index}, S{Song.Index} ==> #{Index}] [{songTime}S ~ {pointTime}P] [{RoundAccuracy}s] {Song.ToString()}";
    }
}

public class StringBuilder
{
    private readonly System.Text.StringBuilder builder;

    public StringBuilder() => builder = new System.Text.StringBuilder();

    public StringBuilder Append(string format, object value)
    {
        if (value != null)
        {
            builder.AppendLine(string.Format(format, value));
        }
        return this;
    }

    public override string ToString() => builder.ToString();
}