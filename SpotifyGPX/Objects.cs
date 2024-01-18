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

            if (DateTimeOffset.TryParseExact(time, Formats.SpotifyMini, null, DateTimeStyles.AssumeUniversal, out var result))
            {
                return result;
            }
            else if (DateTimeOffset.TryParseExact(time, Formats.SpotifyFull, null, DateTimeStyles.AssumeUniversal, out result))
            {
                return result;
            }
            else
            {
                throw new Exception("Couldn't find valid time formats, 'ts' or 'endTime' in JSON");
            }
        }
    }

    public readonly string? Song_Artist => (string?)Json["artistName"] ?? (string?)Json["master_metadata_album_artist_name"];
    public readonly string? Song_Name => (string?)Json["trackName"] ?? (string?)Json["master_metadata_track_name"];
    public readonly string? Time_Played => (string?)Json["msPlayed"] ?? (string?)Json["ms_played"];
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(double.Parse(Time_Played)); // Parse string of milliseconds to TimeSpan
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
    public readonly string? Spotify_OfflineTS => (string?)Json["offline_timestamp"];
    public readonly bool? Spotify_Incognito => (bool?)Json["incognito"];
    public override string ToString() => $"{Song_Artist} - {Song_Name}"; // Display format for this song
}

public readonly struct GPXTrack
{
    public GPXTrack(int? index, string? name, bool gaps, List<GPXPoint> points)
    {
        Track = new TrackInfo(index, name, gaps);
        Points = points;
        Start = Points.Select(point => point.Time).Min();
        End = Points.Select(point => point.Time).Max();
    }

    public readonly TrackInfo Track { get; } // Metadata for this track, including its name and index in a list
    public readonly List<GPXPoint> Points { get; } // Where and when were all the points in this track taken?
    public readonly DateTimeOffset Start { get; } // What time was the earliest point logged?
    public readonly DateTimeOffset End { get; } // What time was the latest point logged?
    public override string ToString()
    {
        StringBuilder builder = new();

        builder.Append($"\n   Name: {Track.Name}");
        builder.Append($"\n   Points: {Points.Count}");
        builder.Append($"\n   Starts: {Start.ToString(Formats.ConsoleTrack)}");
        builder.Append($"\n   Ends: {End.ToString(Formats.ConsoleTrack)}");
        if (Track.Gaps) { builder.Append($"\n   Gap track"); }

        return builder.ToString();
    }// Display format for this track
}

public readonly struct TrackInfo
{
    public TrackInfo(int? index, string? name, bool gaps)
    {
        Indexx = index;
        NodeName = name;
        Gaps = gaps;
    }

    private readonly int? Indexx;
    public readonly int Index
    {
        get
        {
            if (Indexx == null)
            {
                if (Gaps)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return (int)Indexx;
            }
        }
    }
    private readonly string? NodeName;
    public readonly string Name
    {
        get
        {
            if (NodeName == null)
            {
                return $"T{Index}";
            }
            else
            {
                return NodeName;
            }
        }
    }
    public bool Gaps { get; }
    public override string ToString() => Name;
}

public readonly struct GPXPoint
{
    public GPXPoint(int index, Coordinate point, string time)
    {
        Index = index;
        Location = point;
        Time = DateTimeOffset.ParseExact(time, Formats.GpxInput, null);
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

    public readonly double Latitude;
    public readonly double Longitude;
    public override string ToString() => $"{(Latitude, Longitude)}"; // Display format for this coordinate pair
}

public readonly struct SongPoint
{
    public readonly string Description // GPX point description in exported GPX
    {
        get
        {
            StringBuilder builder = new();

            builder.AppendLine($"At this position at {PointTime.ToString(Formats.DescriptionPlayedAt)},");
            builder.AppendLine($"the song {GetRelativeDescription()} {SongTime.ToString(Formats.DescriptionPlayedAt)},");
            builder.AppendLine($"played for {Song.TimePlayed.ToString(Formats.DescriptionTimePlayed)}.");
            builder.AppendLine($"Skipped: {(Song.Song_Skipped == true ? "Yes" : "No")},");
            builder.AppendLine($"Shuffle: {(Song.Song_Shuffle == true ? "On" : "Off")},");
            builder.AppendLine($"Offline: {(Song.Spotify_Offline == true ? "Yes" : "No")},");
            builder.AppendLine($"IP Address: {Song.Spotify_IP},");
            builder.AppendLine($"Country: {Song.Spotify_Country}");

            return builder.ToString();
        }
    }

    private string GetRelativeDescription() // Used to describe the relation of the song to the point
    {
        string seconds = AbsRoundAccuracy == 1 ? "second" : "seconds";

        // accuracy < 0 means song ends before the point
        // accuracy == 0 means song ends in the same place as the point
        // accuracy > 0 means song ends after the point
        if (RoundAccuracy < 0)
        {
            return $"ended {AbsRoundAccuracy} {seconds} before, at";
        }
        else if (RoundAccuracy == 0)
        {
            return "ended at the same time, at";
        }
        else
        {
            return $"ended {AbsRoundAccuracy} {seconds} after, at";
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
    public readonly TrackInfo Origin { get; } // Track from which the pairing originates
    private readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds; // Raw accuracy
    public readonly double AbsAccuracy => Math.Abs(Accuracy); // Absolute value of the accuracy
    private readonly double RoundAccuracy => Math.Round(Accuracy); // Rounded accuracy
    private readonly double AbsRoundAccuracy => Math.Abs(RoundAccuracy); // Absolute value of the rounded accuracy
    private readonly TimeSpan NormalizedOffset => Point.Time.Offset; // Standard offset is defined by the original GPX point offset
    private DateTimeOffset SongTime => Song.Time.ToOffset(NormalizedOffset); // Song end time, normalized to point time zone
    private DateTimeOffset PointTime => Point.Time.ToOffset(NormalizedOffset); // Point end time, normalized to point time zone (redundant)

    public override string ToString()
    {
        // Set both the song and point times to the UTC offset provided by the original GPX point
        string songTime = SongTime.ToString(Formats.Console);
        string pointTime = PointTime.ToString(Formats.Console);

        // Print information about the pairing
        return $"[{Origin}] [P{Point.Index}, S{Song.Index} ==> #{Index}] [{songTime}S ~ {pointTime}P] [{RoundAccuracy}s] {Song}";
    }
}
