using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public class JsonReport : ISongInput, IGpsInput
{
    private List<JObject> JsonTracks { get; }
    private List<SpotifyEntry> AllSongs { get; }
    private List<GPXTrack> AllTracks { get; }

    /// <summary>
    /// Creates a JsonReport importer that allows job creation from existing job reports.
    /// </summary>
    /// <param name="path">The path to a JsonReport file.</param>
    public JsonReport(string path)
    {
        JsonTracks = Deserialize(path);
        (List<GPXTrack> tracks, List<SpotifyEntry> songs) = GetFromJObject();
        AllTracks = tracks;
        AllSongs = songs;
    }

    /// <summary>
    /// Parses a JsonReport to a list of tracks and songs.
    /// </summary>
    /// <returns>A list of tracks and a list of songs representing the included data.</returns>
    /// <exception cref="Exception"></exception>
    public (List<GPXTrack>, List<SpotifyEntry>) GetFromJObject()
    {
        List<GPXTrack> tracks = new();
        List<SpotifyEntry> songs = new();

        int existingPoints = 0;

        foreach (JObject track in JsonTracks)
        {
            // Get the Track item
            int expectedCount = track.Value<int>("Count");
            JObject trackInfo = track.Value<JObject>("TrackInfo") ?? throw new Exception($"No TrackInfo object for track {JsonTracks.IndexOf(track)}");

            // Get the TrackInfo item
            int trackIndex = trackInfo.Value<int>("Index");
            string trackName = trackInfo.Value<string>("Name") ?? throw new Exception($"No Name value in TrackInfo for track {trackIndex}");
            TrackType trackType = (TrackType)trackInfo.Value<int>("Type");
            TrackInfo tInfo = new(trackIndex, trackName, trackType);

            JArray pairsArray = track.Value<JArray>(trackName) ?? throw new Exception($"No pairs tree found with track name '{trackName}'");

            List<GPXPoint> points = new();

            List<int> indexes = new();
            int trackCount = 0;

            // Iterate through each pair
            foreach (JToken pair in pairsArray)
            {
                // Get the Pair item
                int pairIndex = pair.Value<int>("Index");
                indexes.Add(pairIndex);

                // Get the Point item
                JObject point = pair.Value<JObject>("Point") ?? throw new Exception($"Pair {pairIndex} is missing a Point.");
                int index = point.Value<int>("Index");
                JObject coordinate = point.Value<JObject>("Location") ?? throw new Exception($"Pair {pairIndex} is missing a Point/Location.");
                double lat = coordinate.Value<double>("Latitude");
                double lon = coordinate.Value<double>("Longitude");
                string time = point.Value<string>("OriTime") ?? throw new Exception($"Pair {pairIndex} is missing an OriTime.");

                // Create the GPXPoint
                GPXPoint pt = new(index, new Coordinate(lat, lon), time);
                points.Add(pt);

                // Get the Song item
                SpotifyEntry song = pair["Song"].ToObject<SpotifyEntry>();
                songs.Add(song);

                // Get the TrackOrigin item
                JObject origin = pair.Value<JObject>("Origin") ?? throw new Exception($"Pair {pairIndex} is missing an Origin.");
                int pairTrackIndex = origin.Value<int>("Index");
                string pairTrackName = origin.Value<string>("Name") ?? throw new Exception($"Pair {pairIndex} is missing an Origin/Name");
                TrackType pairTrackType = (TrackType)origin.Value<int>("Type");
                TrackInfo pairTinfo = new(pairTrackIndex, pairTrackName, pairTrackType);

                // Verify the TrackOrigin
                if (tInfo != pairTinfo)
                {
                    throw new Exception($"Pair {pairIndex} TrackOrigin does not match origin of parent track {trackIndex}");
                }

                // Verify the Pair index
                trackCount++;
            }

            // Create the GPXTrack
            GPXTrack t = new(trackIndex, trackName, trackType, points);
            tracks.Add(t);

            // Verify the quantity of pairs
            if (trackCount != expectedCount)
            {
                VerifyQuantity(existingPoints, expectedCount, indexes, trackIndex);
            }

            // Update the existingPoints
            existingPoints += trackCount;
        }

        return (tracks, songs);
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
    public int Count => AllSongs.Count;

    /// <summary>
    /// The total number of tracks in the JsonReport file.
    /// </summary>
    public int TrackCount => JsonTracks.Count;

    /// <summary>
    /// The total number of points (across all tracks) in the JsonReport file.
    /// </summary>
    public int PointCount => AllTracks.Select(track => track.Points.Count).Sum();

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
                    var serializer = JsonSerializer.Create(Options.JsonSettings);
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
