// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.Globalization;

#nullable enable

namespace SpotifyGPX.Json;

public partial class Json
{
    public static string ExportSpotifyJson(List<SpotifyEntry> tracks)
    {
        // Create a list of JSON objects
        List<JObject> json = new();

        foreach (SpotifyEntry entry in tracks)
        {
            // Attempt to parse each SpotifyEntry to a JSON object
            try
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["ts"] = entry.Time_End.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    ["username"] = entry.Spotify_Username,
                    ["platform"] = entry.Spotify_Platform,
                    ["ms_played"] = entry.Time_Played,
                    ["conn_country"] = entry.Spotify_Country,
                    ["ip_addr_decrypted"] = entry.Spotify_IP,
                    ["user_agent_decrypted"] = entry.Spotify_UA,
                    ["master_metadata_track_name"] = entry.Song_Name,
                    ["master_metadata_album_artist_name"] = entry.Song_Artist,
                    ["master_metadata_album_album_name"] = entry.Song_Album,
                    ["spotify_track_uri"] = entry.Song_URI,
                    ["episode_name"] = entry.Episode_Name,
                    ["episode_show_name"] = entry.Episode_Show,
                    ["spotify_episode_uri"] = entry.Episode_URI,
                    ["reason_start"] = entry.Song_StartReason,
                    ["reason_end"] = entry.Song_EndReason,
                    ["shuffle"] = entry.Song_Shuffle,
                    ["skipped"] = entry.Song_Skipped,
                    ["offline"] = entry.Spotify_Offline,
                    ["offline_timestamp"] = entry.Spotify_OfflineTS,
                    ["incognito"] = entry.Spotify_Incognito
                };

                // Add the SpotifyEntry JObject to the list
                json.Add(songEntry);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending track, '{entry.Song_Name}', to JSON: {ex.Message}");
            }
        }

        // Create a JSON document based on the list of songs within range
        string document = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);

        return document;
    }
}
