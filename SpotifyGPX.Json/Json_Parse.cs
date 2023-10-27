// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SpotifyGPX.Options;

#nullable enable

namespace SpotifyGPX.Json;

public partial class Json
{
    public static List<SpotifyEntry> ParseSpotifyJson(string jsonFile)
    {
        // Create list of JSON objects
        List<JObject>? sourceJson;

        try
        {
            // Attempt to deserialize JSON file to list
            sourceJson = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFile));
            if (sourceJson == null)
            {
                throw new Exception("Deserializing results in null return! Check your JSON!");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deserializing given JSON file: {ex.Message}");
        }

        // Define time string formats:
        string verboseTimeFormat = "MM/dd/yyyy HH:mm:ss";
        string minifiedTimeFormat = "yyyy-MM-dd HH:mm";

        // Create list to store the parsed Spotify songs
        List<SpotifyEntry> spotifyEntries = sourceJson.Select(track =>
        {
            DateTimeOffset parsedTime = DateTimeOffset.TryParseExact((string?)track["endTime"] ?? (string?)track["ts"], (string?)track["ts"] == null ? minifiedTimeFormat : verboseTimeFormat, null, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : throw new Exception($"Error parsing DateTimeOffset from song end timestamp: \n{track}");

            try
            {
                return new SpotifyEntry
                {
                    Time_End = parsedTime,
                    Spotify_Username = (string?)track["username"],
                    Spotify_Platform = (string?)track["platform"],
                    Time_Played = (string?)track["msPlayed"] ?? (string?)track["ms_played"],
                    Spotify_Country = (string?)track["conn_country"],
                    Spotify_IP = (string?)track["ip_addr_decrypted"],
                    Spotify_UA = (string?)track["user_agent_decrypted"],
                    Song_Name = (string?)track["trackName"] ?? (string?)track["master_metadata_track_name"],
                    Song_Artist = (string?)track["artistName"] ?? (string?)track["master_metadata_album_artist_name"],
                    Song_Album = (string?)track["master_metadata_album_album_name"],
                    Song_URI = (string?)track["spotify_track_uri"],
                    Episode_Name = (string?)track["episode_name"],
                    Episode_Show = (string?)track["episode_show_name"],
                    Episode_URI = (string?)track["spotify_episode_uri"],
                    Song_StartReason = (string?)track["reason_start"],
                    Song_EndReason = (string?)track["reason_end"],
                    Song_Shuffle = (bool?)track["shuffle"],
                    Song_Skipped = (bool?)track["skipped"],
                    Spotify_Offline = (bool?)track["offline"],
                    Spotify_OfflineTS = (string?)track["offline_timestamp"],
                    Spotify_Incognito = (bool?)track["incognito"]
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing contents of JSON tag:\n{track} to a valid song entry:\n{ex.Message}");
            }
        }).ToList();

        return spotifyEntries;
    }
}
