// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Output;

public partial class Json : JsonSaveable
{
    protected override List<JsonDocument> Document { get; }

    public Json(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private List<JsonDocument> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        List<JsonDocument> objects = Pairs
            .Select(pair =>
            {
                return JsonDocument.Parse(JsonSerializer.Serialize(pair.Song, JsonOptions));
            })
            .ToList();

        return objects;
    }

    public override int Count => Document.Count; // Number of JObjects in list
}
