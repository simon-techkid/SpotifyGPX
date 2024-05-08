// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Input;

public sealed partial class JsonReport : PairInputBase, IHashVerifier
{
    private List<JsonDocument> JsonObjects { get; }
    private JsonElement Header => JsonObjects.First().RootElement;
    private List<JsonDocument> JsonTracksOnly { get; }
    protected override ParsePairsDelegate ParsePairsMethod => GetFromJObject;
    protected override FilterSongsDelegate FilterSongsMethod => FilterSongs;

    public JsonReport(string path) : base(path)
    {
        using JsonNetDeserializer deserializer = new(path);
        JsonObjects = deserializer.Deserialize<JsonDocument>(JsonOptions);
        deserializer.Dispose();
        JsonTracksOnly = JsonObjects.Skip(1).ToList();
    }

    private List<SongPoint> GetFromJObject()
    {
        // Get the header
        int expectedTotal = Header.TryGetProperty("Total", out JsonElement total) ? total.GetInt32() : throw new Exception($"Expected total required in header object of JsonReport!");

        List<SongPoint> allPairs = JsonTracksOnly
            .SelectMany((track, index) =>
            {
                JsonElement root = track.RootElement;

                // Get the Count item
                int expectedPairCount = root.TryGetProperty("Count", out JsonElement count) ? count.GetInt32() : throw new Exception($"No Count object found in track {index}");

                // Get the TrackInfo item
                if (!root.TryGetProperty("TrackInfo", out JsonElement trackInfo))
                {
                    throw new Exception($"No TrackInfo object for track {index}");
                }

                // TrackInfo
                int? trackIndex = trackInfo.TryGetProperty("Index", out JsonElement trIndex) ? trIndex.GetInt32() : throw new Exception($"No Index object found in TrackInfo for track {index}");
                string? trackName = trackInfo.TryGetProperty("Name", out JsonElement name) ? name.GetString() : throw new Exception($"No Name object found in TrackInfo for track {index}");
                int? trackType = trackInfo.TryGetProperty("Type", out JsonElement type) ? type.GetInt32() : throw new Exception($"No Type object found in TrackInfo for track {index}");

                // Get the pairs tree
                if (!root.TryGetProperty("Track", out JsonElement pairs))
                {
                    throw new Exception($"No pairs tree found with track name '{trackName}'");
                }

                return pairs.EnumerateArray().Select(pair =>
                {
                    // Index
                    JsonElement indexx = pair.GetProperty("Index");
                    int pairIndex = indexx.GetInt32();

                    // Song
                    JsonElement song = pair.GetProperty("Song");
                    string songDescription = song.GetProperty("Description").GetString() ?? string.Empty;
                    int songIndex = song.GetProperty("Index").GetInt32();
                    DateTimeOffset songTime = song.GetProperty("Time").GetDateTimeOffset();
                    string? songArtist = song.GetProperty("Song_Artist").GetString();
                    string? songName = song.GetProperty("Song_Name").GetString();
                    int songCurrentUsage = song.GetProperty("CurrentUsage").GetInt32();
                    int songCurrentInterpretation = song.GetProperty("CurrentInterpretation").GetInt32();

                    ISongEntry songEntry = new GenericEntry()
                    {
                        Description = songDescription,
                        Index = songIndex,
                        FriendlyTime = songTime,
                        Song_Artist = songArtist,
                        Song_Name = songName,
                        CurrentUsage = (TimeUsage)songCurrentUsage,
                        CurrentInterpretation = (TimeInterpretation)songCurrentInterpretation
                    };

                    // Point
                    JsonElement point = pair.GetProperty("Point");
                    int pointIndex = point.GetProperty("Index").GetInt32();
                    JsonElement pointLocation = point.GetProperty("Location");
                    double pointLocationLatitude = pointLocation.GetProperty("Latitude").GetDouble();
                    double pointLocationLongitude = pointLocation.GetProperty("Longitude").GetDouble();
                    DateTimeOffset pointTime = point.GetProperty("Time").GetDateTimeOffset();

                    IGpsPoint pointEntry = new GenericPoint()
                    {
                        Index = pointIndex,
                        Location = new Coordinate(pointLocationLatitude, pointLocationLongitude),
                        Time = pointTime
                    };

                    // Origin (TrackInfo)
                    JsonElement origin = pair.GetProperty("Origin");
                    int? originIndex = origin.GetProperty("Index").GetInt32();
                    string? originName = origin.GetProperty("Name").GetString();
                    int originType = origin.GetProperty("Type").GetInt32();
                    TrackInfo trackEntry = new(originIndex, originName, (TrackType)originType);

                    return new SongPoint(pairIndex, songEntry, pointEntry, trackEntry);
                }).ToList();

            })
            .ToList();

        return allPairs;
    }

    private List<ISongEntry> FilterSongs()
    {
        return AllPairs.Select(pair => pair.Song).Where(pair => songFilter(pair) == true).ToList();
    }

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

    public override int SourceSongCount => JsonTracksOnly.Select(track => track.RootElement.GetProperty("Track").EnumerateArray().Select(pair => pair.GetProperty("Song")).Count()).Sum();

    public override int SourcePointCount => JsonTracksOnly.Select(track => track.RootElement.GetProperty("Track").EnumerateArray().Select(pair => pair.GetProperty("Point")).Count()).Sum();

    public override int SourceTrackCount => JsonTracksOnly.Select(track => track).Count();

    public override int SourcePairCount => JsonTracksOnly.Select(track => track.RootElement.GetProperty("Track").EnumerateArray().Count()).Sum();

    protected override void ClearDocument()
    {
        JsonObjects.ForEach(obj => obj.Dispose());
        JsonTracksOnly.ForEach(obj => obj.Dispose());
    }

    public bool VerifyHash()
    {
        JsonHashProvider hasher = new();
        string? expectedHash = Header.TryGetProperty("SHA256Hash", out JsonElement hash) ? hash.GetString() : null;
        return hasher.VerifyHash(JsonTracksOnly, expectedHash);
    }
}
