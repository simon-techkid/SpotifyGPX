// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Output;

public sealed partial class JsonReport : JsonSaveable
{
    public override string FormatName => nameof(JsonReport).ToLower();

    public JsonReport(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

    protected override List<JsonDocument> GetDocument(string? trackName)
    {
        IEnumerable<IGrouping<TrackInfo, SongPoint>> tracks = GroupedDataProvider(pair => pair.Origin);

        List<JsonDocument> objects = tracks
            .Select(track =>
            {
                var json = new
                {
                    Count = track.Count(), // Include # of pairs in this track
                    TrackInfo = track.Key, // Include info about the GPX track
                    Track = track.Select(pair => pair) // Create a json report for each pair
                };

                return JsonDocument.Parse(JsonSerializer.Serialize(json));
            })
            .ToList();

        string hash = HashProvider.ComputeHash(objects);

        var header = new
        {
            Created = DateTimeOffset.Now.ToUniversalTime(),
            Tracks = tracks.Count(),
            Total = tracks.SelectMany(track => track).Count(),
            SHA256Hash = hash
        };

        JsonDocument headerDoc = JsonDocument.Parse(JsonSerializer.Serialize(header));

        objects.Insert(0, headerDoc);

        return objects;
    }

    public override int Count
    {
        get
        {
            // For each document (JsonDocument) in Document (List<JsonDocument>),
            // Get that JsonDocument's RootElement
            // Select the last child (in this case, the pair list)
            // Get the count of pairs within the pair list
            // Get the sum of pairs in that JsonDocument
            // Get the sum of pairs in all selected JsonDocuments of List<JsonDocument>

            return Document.Skip(1).Select(JsonDocument => JsonDocument.RootElement.GetProperty("Count").GetInt32()).Sum();
        }
    }
}
