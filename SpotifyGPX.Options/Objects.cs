// SpotifyGPX by Simon Field

using System;

#nullable enable

namespace SpotifyGPX.Options;

public struct SpotifyEntry
{
    public DateTimeOffset Time_End { get; set; }
    public string? Song_Artist { get; set; }
    public string? Song_Name { get; set; }
    public string? Time_Played { get; set; }
    public string? Spotify_Username { get; set; }
    public string? Spotify_Platform { get; set; }
    public string? Spotify_Country { get; set; }
    public string? Spotify_IP { get; set; }
    public string? Spotify_UA { get; set; }
    public string? Song_Album { get; set; }
    public string? Song_URI { get; set; }
    public string? Episode_Name { get; set; }
    public string? Episode_Show { get; set; }
    public string? Episode_URI { get; set; }
    public string? Song_StartReason { get; set; }
    public string? Song_EndReason { get; set; }
    public bool? Song_Shuffle { get; set; }
    public bool? Song_Skipped { get; set; }
    public bool? Spotify_Offline { get; set; }
    public string? Spotify_OfflineTS { get; set; }
    public bool? Spotify_Incognito { get; set; }
}

public struct GPXPoint
{
    public bool? Predicted { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset Time { get; set; }
}
