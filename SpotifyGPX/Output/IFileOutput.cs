// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with file output classes, unifying all formats that pairs can be written out as.
/// </summary>
public interface IFileOutput
{
    void Save(string path); // Allows the saving of that file to the local disk
    int Count { get; } // Provides the number of pairings in the file
}
