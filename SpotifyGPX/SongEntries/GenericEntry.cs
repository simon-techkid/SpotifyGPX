// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using System;

namespace SpotifyGPX;

public struct GenericEntry : ISongEntry
{
    public override readonly string ToString() => $"{Song_Name} - {Song_Artist}";
    public string Description { get; set; }
    public int Index { get; set; }
    public DateTimeOffset FriendlyTime { get; set; }
    public string? Song_Name { get; set; }
    public string? Song_Artist { get; set; }
    public TimeUsage CurrentUsage { get; set; }
    public TimeInterpretation CurrentInterpretation { get; set; }
}
