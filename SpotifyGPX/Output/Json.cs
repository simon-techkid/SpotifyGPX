// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Output;

public sealed partial class Json : JsonSaveable
{
    public override string FormatName => nameof(Json).ToLower();

    public Json(Func<IEnumerable<SongPoint>> pairs, string? trackName, Broadcaster bcast) : base(pairs, trackName, bcast)
    {
    }

    protected override List<JsonDocument> GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> Pairs = DataProvider();

        List<JsonDocument> objects = Pairs
            .Select(pair => JsonDocument.Parse(JsonSerializer.Serialize(pair.Song.GetObject<SpotifyEntry>())))
            .ToList();

        return objects;
    }

    public override int Count => Document.Count; // Number of JObjects in list
}
