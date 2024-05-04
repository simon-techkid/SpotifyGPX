// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public sealed partial class Csv : SongInputBase
{
    private string[] Document { get; }
    protected override List<ISongEntry> AllSongs { get; }

    public Csv(string path)
    {
        Document = File.ReadAllLines(path);
        AllSongs = ParseSongs();
    }

    private List<ISongEntry> ParseSongs()
    {
        return Document.Skip(1).Select((entry, index) =>
        {
            string[] parts = entry.Split("\",\"");

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

    protected override List<ISongEntry> FilterSongs()
    {
        return AllSongs.OfType<LastFmEntry>().Where(song => filter(song)).Select(song => (ISongEntry)song).ToList();
    }

    public override int SourceSongCount => Document.Length - 1;

}
