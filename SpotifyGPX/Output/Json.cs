// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Output;

public sealed partial class Json : JsonSaveable
{
    public override string FormatName => nameof(Json).ToLower();
    protected override List<JsonDocument> Document { get; }

    public Json(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private static List<JsonDocument> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        List<JsonDocument> objects = Pairs
            .Select(pair => JsonDocument.Parse(JsonSerializer.Serialize(pair.Song)))
            .ToList();

        return objects;
    }

    public override int Count => Document.Count; // Number of JObjects in list
}
