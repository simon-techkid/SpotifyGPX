﻿// SpotifyGPX by Simon Field

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
            throw new Exception($"Error parsing contents of JSON tag:\n{json} to a valid song entry:\n{ex}");
        }
    }

    private readonly JObject Json;

    public int Index { get; } // Unique identifier of this song in a list
    public readonly DateTimeOffset Time
    {
        get
        {
            string time = ((string?)Json["endTime"] ?? (string?)Json["ts"]) ?? throw new Exception($"JSON 'ts' or 'endTime' cannot be null, check your JSON");

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

    public readonly string? Song_Artist => (string?)Json["artistName"] ?? (string?)Json["master_metadata_album_artist_name"];
    public readonly string? Song_Name => (string?)Json["trackName"] ?? (string?)Json["master_metadata_track_name"];
    public readonly string? Time_Played => (string?)Json["msPlayed"] ?? (string?)Json["ms_played"];
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(double.Parse(Time_Played));
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
    public override string ToString() => $"{Song_Artist} - {Song_Name}";
}

public readonly struct GPXTrack
{
    public GPXTrack(int index, List<GPXPoint> points, string? name)
    {
        Index = index;
        Points = points;
        NameNode = name;
        Start = Points.Select(point => point.Time).Min();
        End = Points.Select(point => point.Time).Max();
        Console.WriteLine(this);
    }

    public int Index { get; } // All GPXPoints of this track share this index as their TrackIndex
    public List<GPXPoint> Points { get; }
    private readonly string? NameNode;
    public readonly string Name
    {
        get
        {
            if (NameNode == null)
            {
                return $"Track {Index}";
            }
            else
            {
                return NameNode;
            }
        }
    }
    public readonly DateTimeOffset Start { get; }
    public readonly DateTimeOffset End { get; }
    public override string ToString() => $"[{Name}] ({Points.Count} points) Starts: {Start}, Ends: {End}";
}

public readonly struct TrackInfo
{
    public TrackInfo(GPXTrack track)
    {
        Index = track.Index;
        Name = track.Name;
    }

    public int Index { get; }
    public string Name { get; }
}

public readonly struct GPXPoint
{
    public GPXPoint(int trackInd, int index, Coordinate point, string time)
    {
        TrackIndex = trackInd;
        Index = index;
        Location = point;
        Time = DateTimeOffset.ParseExact(time, Formats.gpxTimeInp, null);
    }

    public int Index { get; } // Unique identifier of this point in a list
    public int TrackIndex { get; } // Index of the track this point belongs to
    public Coordinate Location { get; } // Coordinate pair of its location
    public DateTimeOffset Time { get; } // Time of the point
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

    public override string ToString() => $"{(Latitude, Longitude)}";
}

public readonly struct SongPoint
{
    public string Description
    {
        get
        {
            // ===================== \\
            // GPX POINT DESCRIPTION \\
            // ===================== \\

            // Print the song time adjusted for the local time zone provided by the GPX point
            DateTimeOffset EndedAt = Song.Time.ToOffset(EqualizedOffset);

            StringBuilder builder = new();

            // accuracy < 0 means song ends before the point
            // accuracy == 0 means song ends in the same place as the point
            // accuracy > 0 means song ends after the point
            string seconds = AbsRoundAccuracy == 1 ? "second" : "seconds";
            string ppexpl = $"{(RoundAccuracy < 0 ? $"{AbsRoundAccuracy} {seconds} before, at" : RoundAccuracy == 0 ? "at the same time," : $"{AbsRoundAccuracy} {seconds} after, at")}";

            // Begin line contents of the description
            builder.AppendLine($"At this location at {Point.Time.ToString(Formats.gpxDescriptionPlayedAt)}");
            builder.AppendLine($"Song ended {ppexpl} {EndedAt.ToString(Formats.gpxDescriptionPlayedAt)}");
            builder.AppendLine($"Song played for {Song.TimePlayed.ToString(Formats.gpxDescriptionTimePlayed)}");
            builder.AppendLine($"Skipped: {(Song.Song_Skipped == true ? "Yes" : "No")}");
            builder.AppendLine($"Shuffle: {(Song.Song_Shuffle == true ? "On" : "Off")}");
            builder.AppendLine($"Offline: {(Song.Spotify_Offline == true ? "Yes" : "No")}");
            builder.AppendLine($"IP Address: {Song.Spotify_IP}");
            builder.AppendLine($"Country: {Song.Spotify_Country}");

            return builder.ToString();
        }
    }

    public SongPoint(SpotifyEntry song, GPXPoint point, int index, TrackInfo origin)
    {
        Song = song;
        Point = point;
        Index = index;
        Origin = origin;
        Console.WriteLine(this);
    }

    public readonly int Index { get; } // Unique identifier of this SongPoint in a list
    public readonly TrackInfo Origin { get; } // Track from which the pairing originates
    private readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds; // Raw accuracy
    public readonly double AbsAccuracy => Math.Abs(Accuracy); // Absolute value of the accuracy
    private readonly double RoundAccuracy => Math.Round(Accuracy); // Rounded accuracy
    private readonly double AbsRoundAccuracy => Math.Abs(RoundAccuracy); // Absolute value of the rounded accuracy
    public SpotifyEntry Song { get; } // Contents of the original song entry
    public GPXPoint Point { get; } // Contents of the original GPX point
    public readonly TimeSpan EqualizedOffset => Point.Time.Offset; // Offset is defined by the original GPX point offset

    public override string ToString()
    {
        // Set both the song and point times to the UTC offset provided by the original GPX point
        string songTime = Song.Time.ToOffset(EqualizedOffset).ToString(Formats.consoleReadoutFormat);
        string pointTime = Point.Time.ToOffset(EqualizedOffset).ToString(Formats.consoleReadoutFormat);

        // Print information about the pairing
        return $"[{Origin.Name}#{Point.Index} ==> {Index}] [{songTime} ~ {pointTime}] [{RoundAccuracy}s] {Song}";
    }
}