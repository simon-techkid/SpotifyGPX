using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Txt : IFileOutput
{
    public static bool SupportsMultiTrack => false; // Does this file format allow multiple GPXTracks to be contained?
    public Txt(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private string[] Document { get; }

    private static string[] GetDocument(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair => pair.Song.Song_URI).Where(s => s != null).ToArray();
    }

    public void Save(string path)
    {
        File.WriteAllLines(path, Document);
    }

    public int Count => Document.Length;
}
