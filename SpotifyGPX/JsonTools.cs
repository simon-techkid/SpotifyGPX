// SpotifyGPX by Simon Field

using System;
using System.Text.Json;

namespace SpotifyGPX;

public class JsonTools
{
    public static JsonElement? TryGetProperty(string name, JsonElement parent)
    {
        return parent.TryGetProperty(name, out JsonElement property) ? property : null;
    }

    public static JsonElement ForceGetProperty(string name, JsonElement parent)
    {
        return parent.TryGetProperty(name, out JsonElement property) ? property : throw new Exception($"No {name} object found in {parent}");
    }
}
