// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing song playback data and GPS data from the JsonReport format.
/// </summary>
public partial class JsonReport : ISongInput, IGpsInput, IPairInput
{
    private List<JObject> JsonTracks { get; }
    private List<SpotifyEntry> AllSongs { get; }
    private List<GPXTrack> AllTracks { get; }
    private List<SongPoint> AllPairs { get; }

    /// <summary>
    /// Creates a JsonReport importer that allows job creation from existing job reports.
    /// </summary>
    /// <param name="path">The path to a JsonReport file.</param>
    public JsonReport(string path)
    {
        JsonTracks = Deserialize(path);
        (List<GPXTrack> tracks, List<SpotifyEntry> songs, List<SongPoint> pairs) = GetFromJObject();
        AllTracks = tracks;
        AllSongs = songs;
        AllPairs = pairs;
    }

    /// <summary>
    /// Parses a JsonReport to a list of tracks and songs.
    /// </summary>
    /// <returns>A list of tracks and a list of songs representing the included data.</returns>
    /// <exception cref="Exception"></exception>
    public (List<GPXTrack>, List<SpotifyEntry>, List<SongPoint>) GetFromJObject()
    {
        List<GPXTrack> tracks = new(); // All tracks in the JsonReport
        List<SpotifyEntry> allSongs = new(); // All songs in the JsonReport
        List<SongPoint> allPairs = new(); // All pairs in the JsonReport

        int alreadyParsed = 0; // The number of points already parsed

        // Get the header
        JObject header = JsonTracks.First();
        int expectedGpx = header.Value<int>("0");
        int actualGpx = 0;
        int expectedGap = header.Value<int>("1");
        int actualGap = 0;
        int expectedCombined = header.Value<int>("2");
        int actualCombined = 0;
        int total = header.Value<int>("Total");

        // Verify the header
        if (expectedGpx + expectedGap + expectedCombined != total)
        {
            throw new Exception($"File header pair track types total invalid!");
        }

        // Iterate through each track
        foreach (JObject track in JsonTracks.Skip(1)) // Skip the header track
        {
            // Get the Count item
            int expectedPairCount = track.Value<int>("Count");

            // Get the TrackInfo item
            JObject trackInfo = track.Value<JObject>("TrackInfo") ?? throw new Exception($"No TrackInfo object for track {JsonTracks.IndexOf(track)}");
            int trackIndex = trackInfo.Value<int>("Index");
            string trackName = trackInfo.Value<string>("Name") ?? throw new Exception($"No Name value in TrackInfo for track {trackIndex}");
            TrackType trackType = (TrackType)trackInfo.Value<int>("Type");
            TrackInfo tInfo = new(trackIndex, trackName, trackType);

            // Get the track pairs
            JArray pairs = track.Value<JArray>(trackName) ?? throw new Exception($"No pairs tree found with track name '{trackName}'");

            // Create the lists
            List<GPXPoint> trackPoints = new();
            List<SpotifyEntry> trackSongs = new();
            List<SongPoint> trackPairs = new();

            List<int> indexes = new();
            int actualPairCount = 0;

            // Iterate through each pair
            foreach (JToken pair in pairs)
            {
                // Get the Index item
                int pairIndex = pair.Value<int>("Index");
                indexes.Add(pairIndex);

                // Get the Point item
                JObject point = pair.Value<JObject>("Point") ?? throw new Exception($"Pair {pairIndex} is missing a Point.");
                int index = point.Value<int>("Index");
                DateTimeOffset time = point.Value<DateTimeOffset?>("Time") ?? throw new Exception($"Pair {pairIndex} is missing an OriTime.");
                JObject coordinate = point.Value<JObject>("Location") ?? throw new Exception($"Pair {pairIndex} is missing a Point/Location.");
                double lat = coordinate.Value<double>("Latitude");
                double lon = coordinate.Value<double>("Longitude");
                Coordinate location = new(lat, lon);
                GPXPoint pairPoint = new(index, location, time);
                trackPoints.Add(pairPoint);

                // Get the Song item
                JObject song = pair.Value<JObject>("Song") ?? throw new Exception($"Pair {pairIndex} is missing a Song");
                SpotifyEntry pairSong = song.ToObject<SpotifyEntry>();
                trackSongs.Add(pairSong);

                // Get the TrackOrigin item
                JObject origin = pair.Value<JObject>("Origin") ?? throw new Exception($"Pair {pairIndex} is missing an Origin.");
                int pairTrackIndex = origin.Value<int>("Index");
                string pairTrackName = origin.Value<string>("Name") ?? throw new Exception($"Pair {pairIndex} is missing an Origin/Name");
                TrackType pairTrackType = (TrackType)origin.Value<int>("Type");
                TrackInfo pairTinfo = new(pairTrackIndex, pairTrackName, pairTrackType);

                // Create the SongPoint
                SongPoint thisPair = new(pairIndex, pairSong, pairPoint, pairTinfo);
                trackPairs.Add(thisPair);

                // Verify the TrackOrigin
                if (tInfo != pairTinfo)
                {
                    throw new Exception($"Pair {pairIndex} TrackOrigin does not match origin of parent track {trackIndex}");
                }

                actualPairCount++;
            }

            // Create the GPXTrack
            GPXTrack t = new(trackIndex, trackName, trackType, trackPoints);
            tracks.Add(t);

            // Add the songs to the list
            allSongs.AddRange(trackSongs);

            // Add the pairs to the list
            allPairs.AddRange(trackPairs);

            // Verify the quantity of pairs
            if (actualPairCount != expectedPairCount)
            {
                VerifyQuantity(alreadyParsed, expectedPairCount, indexes, trackIndex);
            }

            // Update the actual pair counts
            if (trackType == TrackType.GPX)
            {
                actualGpx += actualPairCount;
            }
            else if (trackType == TrackType.Gap)
            {
                actualGap += actualPairCount;
            }
            else if (trackType == TrackType.Combined)
            {
                actualCombined += actualPairCount;
            }

            alreadyParsed += actualPairCount; // Add the number of pairs in this track
        }

        // Verify the total pair counts
        if (actualGpx != expectedGpx || actualGap != expectedGap || actualCombined != expectedCombined)
        {
            throw new Exception($"File pair track types total invalid!");
        }
        else if (alreadyParsed != total)
        {
            throw new Exception($"File pair total invalid!");
        }

        // Return the tracks and songs
        return (tracks, allSongs, allPairs);
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
    public int SongCount => AllSongs.Count;

    /// <summary>
    /// The total number of tracks in the JsonReport file.
    /// </summary>
    public int TrackCount => JsonTracks.Count;

    /// <summary>
    /// The total number of points (across all tracks) in the JsonReport file.
    /// </summary>
    public int PointCount => AllTracks.Select(track => track.Points.Count).Sum();

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
        return AllTracks;
    }

    /// <summary>
    /// Gets a list of all the songs in the JsonReport file.
    /// </summary>
    /// <returns>A list of SpotifyEntries represented in the file, in their original order.</returns>
    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    /// <summary>
    /// Gets a list of all the pairs in the JsonReport file.
    /// </summary>
    /// <returns>A list of SongPoint objects, each comprising a paired song and point.</returns>
    public List<SongPoint> GetAllPairs()
    {
        return AllPairs;
    }

    /// <summary>
    /// Gets a list of the first-level JSON objects in the JsonReport.
    /// </summary>
    /// <param name="jsonFilePath">The path to a JsonReport file.</param>
    /// <returns>A list of JObjects where each track is a JObject.</returns>
    private static List<JObject> Deserialize(string jsonFilePath)
    {
        List<JObject> tracks = new();

        using (var fileStream = File.OpenRead(jsonFilePath))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    var serializer = JsonSerializer.Create(JsonSettings);
                    var json = serializer.Deserialize<JObject>(jsonReader);
                    if (json != null)
                    {
                        tracks.Add(json);
                    }
                }
            }

        return tracks;
    }
}
