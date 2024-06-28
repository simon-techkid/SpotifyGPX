// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using SpotifyGPX.SongInterfaces;
using System;

namespace SpotifyGPX.SongEntry;

public partial struct XspfEntry : ISongEntry, IEstimatableSong
{
    public readonly override string ToString() => $"{Song_Artist} - {Song_Name}";
    public readonly string Description
    {
        get
        {
            StringBuilder builder = new();

            builder.Append("Played for {0}", Time_Played);

            return builder.ToString();
        }
    }
    public int Index { get; set; }
    public DateTimeOffset FriendlyTime { get; set; }
    public TimeInterpretation CurrentInterpretation { get; set; }
    public readonly TimeUsage CurrentUsage => timeUsage;
    public int Time_Played { get; set; }
    public readonly TimeSpan TimePlayed => TimeSpan.FromMilliseconds(Time_Played);
    public string? Song_Artist { get; set; }
    public string? Song_Name { get; set; }
    public string? Song_URI { get; set; }
}
