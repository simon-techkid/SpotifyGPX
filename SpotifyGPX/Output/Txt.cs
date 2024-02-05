using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Output;

public class Txt : OutputHandler.IFileOutput
{
    public static bool SupportsMultiTrack => false;
    public Txt(IEnumerable<SongPoint> pairs) => Document = GetUris(pairs);

    private string[] Document { get; }

    private static string[] GetUris(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair => pair.Song.Song_URI).Where(s => s != null).ToArray();
    }

    public void Save(string path)
    {
        File.WriteAllLines(path, Document);
    }

    public int Count => Document.Length;
}
