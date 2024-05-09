// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SpotifyGPX.Input;

public class JsonNetDeserializer : FileInputBase
{
    private string? Document { get; set; }
    protected override string FormatName => nameof(JsonNetDeserializer);

    public JsonNetDeserializer(string path, Broadcaster bcast) : base(path, bcast)
    {
        Document = StreamReader.ReadToEnd();
    }

    public List<T> Deserialize<T>(JsonSerializerOptions options)
    {
        if (Document != null)
        {
            List<T> objects = JsonSerializer.Deserialize<List<T>>(Document, options) ?? new List<T>();

            return objects;
        }

        return new List<T>();
    }

    public JsonDocument GetDocument()
    {
        if (Document != null)
        {
            return JsonDocument.Parse(Document);
        }

        return JsonDocument.Parse("{}");
    }

    protected override void DisposeDocument()
    {
        Document = null;
    }

}
