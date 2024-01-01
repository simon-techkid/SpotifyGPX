// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text;

#nullable enable

namespace SpotifyGPX.Options;

public struct SpotifyEntry
{
    public SpotifyEntry(JObject track)
    {
        try
        {
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
            throw new Exception($"Error parsing contents of JSON tag:\n{track} to a valid song entry:\n{ex.Message}");
        }
    }

    public int Index { get; set; }
    public DateTimeOffset Time { get; private set; }
    public string TimeStr
    {
        readonly get
        {
            return Time.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
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

    public string? Song_Artist { get; private set; }
    public string? Song_Name { get; private set; }
    public string? Time_Played { get; private set; }
    public string? Spotify_Username { get; private set; }
    public string? Spotify_Platform { get; private set; }
    public string? Spotify_Country { get; private set; }
    public string? Spotify_IP { get; private set; }
    public string? Spotify_UA { get; private set; }
    public string? Song_Album { get; private set; }
    public string? Song_URI { get; private set; }
    public string? Episode_Name { get; private set; }
    public string? Episode_Show { get; private set; }
    public string? Episode_URI { get; private set; }
    public string? Song_StartReason { get; private set; }
    public string? Song_EndReason { get; private set; }
    public bool? Song_Shuffle { get; private set; }
    public bool? Song_Skipped { get; private set; }
    public bool? Spotify_Offline { get; private set; }
    public string? Spotify_OfflineTS { get; private set; }
    public bool? Spotify_Incognito { get; private set; }
}

public struct GPXPoint
{
    public bool Predicted { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset Time { get; set; }
    public string TimeStr
    {
        readonly get => Time.ToString(Point.gpxTimeOut);
        set => Time = DateTimeOffset.ParseExact(value, Point.gpxTimeInp, null);
    }
    public int TrackMember { get; set; }
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
        builder.AppendLine($"{(Point.Predicted == true ? $"Point Predicted" : null)}");

        return builder.ToString();
    }

    public SongPoint(SpotifyEntry song, GPXPoint point)
    {
        Song = song;
        Point = point;
    }

    public readonly double Accuracy => (Song.Time - Point.Time).TotalSeconds;
    public readonly double AbsAccuracy => Math.Abs((Point.Time - Song.Time).TotalSeconds);
    public SpotifyEntry Song { get; }
    public GPXPoint Point { get; }

    public override string ToString()
    {
        return $"[CORR] [{Point.TrackMember}] [{Song.Index}] [{Song.Time.ToUniversalTime().ToString(Options.Point.consoleReadoutFormat)} ~ {Point.Time.ToUniversalTime().ToString(Options.Point.consoleReadoutFormat)}] [~{Math.Round(Accuracy)}s] {GpxTitle()}";
    }
}
