// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpotifyGPX.Input;

public sealed partial class Csv : SongInputBase
{
    private static readonly Regex CSVRegex = MyRegex();
    private string[] Document { get; set; }
    protected override string FormatName => nameof(Csv);
    public override List<ISongEntry> ParseSongsMethod() => ParseSongs();
    public override List<ISongEntry> FilterSongsMethod() => FilterSongs();

    public Csv(string path, StringBroadcaster bcast) : base(path, bcast)
    {
        Document = ReadAllLines();
    }

    private string[] ReadAllLines()
    {
        var lines = new List<string>();
        string line;
        while ((line = StreamReader.ReadLine()!) != null)
        {
            lines.Add(line);
        }
        return lines.ToArray();
    }

    private List<ISongEntry> ParseSongs()
    {
        return Document.Skip(1).Select((entry, index) =>
        {
            string[] parts = CSVRegex
            .Split(entry)
            .Select(field => field.Trim('"'))
            .ToArray();

            return (ISongEntry)new LastFmEntry()
            {
                Index = index,
                FriendlyTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[0])),
                Utc_Time = parts[1],
                Song_Artist = parts[2],
                Mbid_Artist = parts[3],
                Song_Album = parts[4],
                Mbid_Album = parts[5],
                Song_Name = parts[6],
                Mbid_Track = parts[7],
                CurrentInterpretation = Interpretation
            };
        }).ToList();
    }

    private List<ISongEntry> FilterSongs()
    {
        return AllSongs
            .OfType<LastFmEntry>()
            .Where(song => filter(song))
            .Select(song => (ISongEntry)song)
            .ToList();
    }

    protected override void DisposeDocument()
    {
        Document = Array.Empty<string>();
    }

    public override int SourceSongCount => Document.Length - 1;

    [GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")]
    private static partial Regex MyRegex();
}
