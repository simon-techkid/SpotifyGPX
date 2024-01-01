// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

#nullable enable

namespace SpotifyGPX.Json;

public readonly struct JsonFile
{
    private readonly string jsonFilePath;
    private readonly List<JObject> JsonContents => JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFilePath));

    public JsonFile(string path) => jsonFilePath = path;

    private readonly List<SpotifyEntry> SpotifyEntries => JsonContents.Select(track => new SpotifyEntry(track)).ToList();

    public readonly List<SpotifyEntry> FilterSpotifyJson(List<GPXPoint> gpxPoints)
    {
        // Create list of Spotify songs covering the tracked GPX path timeframe
        List<SpotifyEntry> spotifyEntryCandidates = new();

        try
        {
            // Create a dictionary to store the start and end times for each track
            Dictionary<int, (DateTimeOffset, DateTimeOffset)> trackStartEndTimes = new();

            // Calculate start and end times for each track based on GPX points
            foreach (GPXPoint point in gpxPoints)
            {
                int trackIndex = point.TrackMember;

                if (!trackStartEndTimes.ContainsKey(trackIndex))
                {
                    // Initialize start and end times for the track
                    trackStartEndTimes[trackIndex] = (point.Time, point.Time);
                }
                else
                {
                    // Update start and end times as needed
                    if (point.Time < trackStartEndTimes[trackIndex].Item1)
                    {
                        trackStartEndTimes[trackIndex] = (point.Time, trackStartEndTimes[trackIndex].Item2);
                    }
                    if (point.Time > trackStartEndTimes[trackIndex].Item2)
                    {
                        trackStartEndTimes[trackIndex] = (trackStartEndTimes[trackIndex].Item1, point.Time);
                    }
                }
            }

            // Filter Spotify entries based on track-specific start and end times

            return SpotifyEntries
            .Where(entry =>
            {
                // Determine the associated track for each Spotify entry based on its timestamp
                int associatedTrack = -1; // Default value indicating no associated track
                DateTimeOffset entryTime = entry.Time;

                foreach (var trackTimes in trackStartEndTimes)
                {
                    if (entryTime >= trackTimes.Value.Item1 && entryTime <= trackTimes.Value.Item2)
                    {
                        associatedTrack = trackTimes.Key;
                        break; // Exit the loop as soon as an associated track is found
                    }
                }

                // Filter entries associated with a track
                return associatedTrack != -1;
            })
            .Select((song, index) =>
            {
                song.Index = index;
                return song;
            })
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
        }
    }

    public string ExportSpotifyJson(List<SpotifyEntry> songEntries)
    {
        // Create a list of JSON objects
        List<JObject> json = new();

        foreach (SpotifyEntry entry in songEntries)
        {
            // Attempt to parse each SpotifyEntry to a JSON object
            try
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["ts"] = entry.TimeStr,
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

    public static string GenerateClipboardData(List<SpotifyEntry> songEntries)
    {
        // Create string for final clipboard contents
        string clipboard = "";

        foreach (SpotifyEntry entry in songEntries)
        {
            // Ensures no null values return
            if (entry.Song_URI != null)
            {
                clipboard += $"{entry.Song_URI}\n";
            }
            else
            {
                // If null URI, throw exception
                throw new Exception($"URI null for track '{entry.Song_Name}'");
            }
        }

        // Return final clipboard contents
        return clipboard;
    }

    public static XmlDocument CreatePlist(List<SpotifyEntry> tracks, string plistFile)
    {
        // Create a new XML document
        XmlDocument document = new();

        // Create the XML header
        XmlNode header = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(header);

        // Create the XSPF header
        XmlElement XSPF = document.CreateElement("playlist");
        document.AppendChild(XSPF);

        // Add XSPF header attributes
        XSPF.SetAttribute("version", "1.0");
        XSPF.SetAttribute("xmlns", "http://xspf.org/ns/0/");

        // Set the name of the XSPF playlist to the name of the file
        XmlElement name = document.CreateElement("name");
        name.InnerText = Path.GetFileNameWithoutExtension(plistFile);
        XSPF.AppendChild(name);

        // Set the title of the XSPF playlist to the name of the file
        XmlElement creator = document.CreateElement("creator");
        creator.InnerText = "SpotifyGPX";
        XSPF.AppendChild(creator);

        // Create the trackList header
        XmlElement trackList = document.CreateElement("trackList");
        XSPF.AppendChild(trackList);

        foreach (SpotifyEntry entry in tracks)
        {
            // Create track for each song
            XmlElement track = document.CreateElement("track");
            trackList.AppendChild(track);

            // Set the creator of the track to the song artist
            XmlElement artist = document.CreateElement("creator");
            artist.InnerText = entry.Tag(SpotifyEntry.ReturnTag.Creator);
            track.AppendChild(artist);

            // Set the title of the track to the song name
            XmlElement title = document.CreateElement("title");
            title.InnerText = entry.Tag(SpotifyEntry.ReturnTag.Title);
            track.AppendChild(title);

            // Set the annotation of the song to the end time
            XmlElement annotation = document.CreateElement("annotation");
            annotation.InnerText = entry.Tag(SpotifyEntry.ReturnTag.Annotation);
            track.AppendChild(annotation);

            // Set the duration of the song to the amount of time it was listened to
            XmlElement duration = document.CreateElement("duration");
            duration.InnerText = entry.Tag(SpotifyEntry.ReturnTag.Duration);
            track.AppendChild(duration);
        }

        return document;
    }
}