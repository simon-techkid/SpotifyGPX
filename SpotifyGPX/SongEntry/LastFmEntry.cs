// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using System;

namespace SpotifyGPX.SongEntry;

public partial struct LastFmEntry : ISongEntry
{
    public override readonly string ToString() => $"{Song_Artist} - {Song_Name}";

    public readonly string Description
    {
        get
        {
            StringBuilder builder = new();

            return builder.ToString();
        }
    }

    public int Index { get; set; }
    public DateTimeOffset FriendlyTime { get; set; }
    public string? Utc_Time { get; set; }
    public string? Song_Artist { get; set; }
    public string? Mbid_Artist { get; set; }
    public string? Song_Album { get; set; }
    public string? Mbid_Album { get; set; }
    public string? Song_Name { get; set; }
    public string? Mbid_Track { get; set; }
    public TimeInterpretation CurrentInterpretation { get; set; }
    public TimeUsage CurrentUsage => timeUsage;
}
