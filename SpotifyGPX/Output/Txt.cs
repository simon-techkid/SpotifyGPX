using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Txt
{
    public Txt(IEnumerable<SongPoint> pairs) => Document = GetUris(pairs);

    private string[] Document { get; }

    private static string[] GetUris(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair => pair.Song.Song_URI).Where(s => s != null).ToArray();
    }

    private int Count => Document.Length;

    public void Save(string path)
    {
        File.WriteAllLines(path, Document);
        Console.WriteLine(ToString());
    }

    public override string ToString() => $"[FILE] TXT file containing {Count} points saved!";
}
