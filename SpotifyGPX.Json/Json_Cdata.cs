// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Json;

public partial class Json
{
    // move to SpotifyGpx.Clipboard
    public static string GenerateClipboardData(List<SpotifyEntry> tracks)
    {
        // Create string for final clipboard contents
        string clipboard = "";

        foreach (SpotifyEntry track in tracks)
        {
            // Ensures no null values return
            if (track.Song_URI != null)
            {
                clipboard += $"{track.Song_URI}\n";
            }
            else
            {
                // If null URI, throw exception
                throw new Exception($"URI null for track '{track.Song_Name}'");
            }
        }

        // Return final clipboard contents
        return clipboard;
    }
}
