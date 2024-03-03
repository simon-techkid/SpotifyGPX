// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing song playback data and GPS data from the JsonReport format.
/// </summary>
public partial class JsonReport : ISongInput, IGpsInput, IPairInput, IJsonDeserializer
{
    private JsonDeserializer JsonDeserializer { get; }
    private List<JObject> JsonTracks { get; }
    private List<SongPoint> AllPairs { get; }

    /// <summary>
    /// Creates a JsonReport importer that allows job creation from existing job reports.
    /// </summary>
    /// <param name="path">The path to a JsonReport file.</param>
    public JsonReport(string path)
    {
        JsonDeserializer = new JsonDeserializer(path, JsonSettings);
        JsonTracks = Deserialize();
        List<SongPoint> pairs = GetFromJObject();
        AllPairs = pairs;
    }

    public List<JObject> Deserialize()
    {
        return JsonDeserializer.Deserialize();
    }

    /// <summary>
    /// Parses a JsonReport to a list of tracks and songs.
    /// </summary>
    /// <returns>A list of tracks and a list of songs representing the included data.</returns>
    /// <exception cref="Exception"></exception>
    public List<SongPoint> GetFromJObject()
    {
        int alreadyParsed = 0; // The number of points already parsed

        // Get the header
        JObject header = JsonTracks.First();
        int expectedTotal = header.Value<int>("Total");

        List<SongPoint> allPairs = JsonTracks
            .Skip(1).SkipLast(1)
            .SelectMany(track =>
            {
                // Get the Count item
                int expectedPairCount = track.Value<int>("Count");

                // Get the TrackInfo item
                JObject trackInfo = track.Value<JObject>("TrackInfo") ?? throw new Exception($"No TrackInfo object for track {JsonTracks.IndexOf(track)}");
                TrackInfo tInfo = trackInfo.ToObject<TrackInfo>();

                // Get the track pairs
                JArray pairs = track.Value<JArray>(tInfo.Name) ?? throw new Exception($"No pairs tree found with track name '{tInfo.Name}'");

                List<SongPoint> trackPairs = pairs
                    .Select(pair => pair.ToObject<SongPoint>())
                    .Where((pair, index) => pair.Origin == tInfo && pair.Index - alreadyParsed == index)
                    .ToList();

                List<int> indexes = trackPairs.Select(pair => pair.Index).ToList();

                // Verify the quantity of pairs
                if (trackPairs.Count != expectedPairCount)
                {
                    VerifyQuantity(alreadyParsed, expectedPairCount, indexes, tInfo.Index);
                }

                alreadyParsed += trackPairs.Count; // Add the number of pairs in this track

                return trackPairs;
            })
            .ToList();

        JsonHashProvider<IEnumerable<JObject>> hasher = new();
        string actualHash = hasher.ComputeHash(JsonTracks.SkipLast(1));

        string expectedHash = JsonTracks.Last().Value<string>("SHA256Hash") ?? throw new Exception($"Hash missing from end of file.");
        Console.WriteLine($"Hashes: expected: {expectedHash} actual: {actualHash}");

        return allPairs;
    }

    /// <summary>
    /// Verifies per-track pair quantities match JSON metadata fields.
    /// </summary>
    /// <param name="start">The index of the first pair in this track.</param>
    /// <param name="expectedCount">The expected number of pairs in this track.</param>
    /// <param name="indexes">The list of pair indexes represented in the JsonReport.</param>
    /// <param name="trackIndex">The index of this track in the entire JsonReport file.</param>
    /// <exception cref="Exception"></exception>
    private static void VerifyQuantity(int start, int expectedCount, List<int> indexes, int trackIndex)
    {
        // Find missing indexes
        List<int> expectedIndexes = Enumerable.Range(start, expectedCount).ToList();
        List<int> missingIndexes = expectedIndexes.Where(index => !indexes.Contains(index)).ToList();

        // Output missing indexes
        string missing = string.Join(", ", missingIndexes.Select(index => index.ToString()));
        Console.WriteLine($"Missing indexes: {missing}");
        throw new Exception($"Track {trackIndex} in JsonReport expected to have {expectedCount} pairs, but had {indexes.Count}");
    }

    /// <summary>
    /// The total number of songs in the JsonReport file.
    /// </summary>
    public int SongCount => AllPairs.Select(pair => pair.Song).Count();

    /// <summary>
    /// The total number of tracks in the JsonReport file.
    /// </summary>
    public int TrackCount => JsonTracks.Count;

    /// <summary>
    /// The total number of points (across all tracks) in the JsonReport file.
    /// </summary>
    public int PointCount => AllPairs.Select(pair => pair.Point).Count();

    /// <summary>
    /// The total number of pairs in the JsonReport file.
    /// </summary>
    public int PairCount => AllPairs.Count;

    /// <summary>
    /// Gets a list of all the tracks in the JsonReport file.
    /// </summary>
    /// <returns>A list of GPXTracks represented in the file, in their original order.</returns>
    public List<GPXTrack> GetAllTracks()
    {
        return AllPairs.GroupBy(pair => pair.Origin).Select(type => new GPXTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
    }

    /// <summary>
    /// Gets a list of all the songs in the JsonReport file.
    /// </summary>
    /// <returns>A list of SpotifyEntries represented in the file, in their original order.</returns>
    public List<SpotifyEntry> GetAllSongs()
    {
        return AllPairs.Select(pair => pair.Song).ToList();
    }

    /// <summary>
    /// Gets a list of all the pairs in the JsonReport file.
    /// </summary>
    /// <returns>A list of SongPoint objects, each comprising a paired song and point.</returns>
    public List<SongPoint> GetAllPairs()
    {
        return AllPairs;
    }
}
