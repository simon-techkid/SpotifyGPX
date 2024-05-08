// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SpotifyGPX.Input;

public class JsonNetDeserializer : FileInputBase
{
    private string? Document { get; set; }

    public JsonNetDeserializer(string path) : base(path)
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

    protected override void ClearDocument()
    {
        Document = null;
    }

}
