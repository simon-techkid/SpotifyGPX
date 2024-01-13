// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX;

public readonly struct Pairings
{
    public Pairings(List<SpotifyEntry> s, List<GPXTrack> t) => PairedPoints = PairPoints(s, t);

    private readonly List<SongPoint> PairedPoints;

    private readonly List<SpotifyEntry> Songs => PairedPoints.Select(pair => pair.Song).ToList();

    private readonly List<GPXPoint> Points => PairedPoints.Select(pair => pair.Point).ToList();

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXTrack> tracks)
    {
        // Correlate Spotify entries with the nearest GPX points

        List<SongPoint> correlatedEntries = tracks // For each GPX track
        .SelectMany(track => // Select the track
            songs.Where(spotifyEntry => // For all entries in SpotifyEntry
                    (spotifyEntry.Time >= track.Start) && (spotifyEntry.Time <= track.End)) // If the Spotify entry falls within the boundaries of the track
                .Select((spotifyEntry, index) => // Select the Spotify entry (and its index within the JSON) if it falls in range of the GPX track
                {
                    GPXPoint nearestPoint = track.Points // For all the points in the track
                        .OrderBy(point => Math.Abs((point.Time - spotifyEntry.Time).TotalSeconds)) // Find the nearest point to the current song
                        .First();

                    return new SongPoint(spotifyEntry, nearestPoint, index, new TrackInfo(track)); // Create a pairing
                })
        )
        .ToList();

        if (correlatedEntries.Count > 0)
        {
            // Calculate and print the average correlation accuracy in seconds
            Console.WriteLine($"[PAIR] Song-Point Correlation Accuracy (avg sec): {Math.Round(correlatedEntries.Average(correlatedPair => correlatedPair.AbsAccuracy))}");
        }

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }

    public readonly void PrintTracks()
    {
        string countsJoined = string.Join(", ", PairedPoints.GroupBy(pair => pair.Origin).Select(track => $"{track.Count()} ({track.Key.Name})"));

        Console.WriteLine($"[PAIR] Paired: {countsJoined}");
    }

    public readonly XDocument GetGpxx(string name, string desc, XNamespace ns)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("xmlns", ns),
                new XAttribute("creator", "SpotifyGPX"),
                new XElement(ns + "name", name),
                new XElement(ns + "desc", desc),
                PairedPoints.GroupBy(pair => pair.Origin).Select(track =>
                    new XElement(ns + "trk",
                        new XElement(ns + "name", track.Key.Name),
                        new XElement(ns + "trkseg",
                            track.Select(pair =>
                                new XElement(ns + "trkpt",
                                    new XAttribute("lat", pair.Point.Location.Latitude),
                                    new XAttribute("lon", pair.Point.Location.Longitude),
                                    new XElement(ns + "name", pair.Song),
                                    new XElement(ns + "time", pair.Point.Time.ToUniversalTime().ToString(Formats.gpxTimeOut)),
                                    new XElement(ns + "desc", pair.Description)
                                )
                            )
                        )
                    )
                )
            )
        );
    }

    public readonly XDocument GetGpx(string name, string desc, XNamespace ns)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("xmlns", ns),
                new XAttribute("creator", "SpotifyGPX"),
                new XElement(ns + "name", name),
                new XElement(ns + "time", DateTime.Now.ToUniversalTime().ToString(Formats.gpxTimeOut)),
                new XElement(ns + "desc", desc),
                PairedPoints.Select(pair =>
                    new XElement(ns + "wpt",
                        new XAttribute("lat", pair.Point.Location.Latitude),
                        new XAttribute("lon", pair.Point.Location.Longitude),
                        new XElement(ns + "name", pair.Song),
                        new XElement(ns + "time", pair.Point.Time.ToUniversalTime().ToString(Formats.gpxTimeOut)),
                        new XElement(ns + "desc", pair.Description)
                    )
                )
            )
        );
    }

    public readonly List<JObject> GetJson()
    {
        // Create a list of JSON objects
        return Songs.Select(song =>
        {
            try
            {
                return new JObject
                {
                    ["ts"] = song.Time.ToString(Formats.outJsonFormat),
                    ["username"] = song.Spotify_Username,
                    ["platform"] = song.Spotify_Platform,
                    ["ms_played"] = song.Time_Played,
                    ["conn_country"] = song.Spotify_Country,
                    ["ip_addr_decrypted"] = song.Spotify_IP,
                    ["user_agent_decrypted"] = song.Spotify_UA,
                    ["master_metadata_track_name"] = song.Song_Name,
                    ["master_metadata_album_artist_name"] = song.Song_Artist,
                    ["master_metadata_album_album_name"] = song.Song_Album,
                    ["spotify_track_uri"] = song.Song_URI,
                    ["episode_name"] = song.Episode_Name,
                    ["episode_show_name"] = song.Episode_Show,
                    ["spotify_episode_uri"] = song.Episode_URI,
                    ["reason_start"] = song.Song_StartReason,
                    ["reason_end"] = song.Song_EndReason,
                    ["shuffle"] = song.Song_Shuffle,
                    ["skipped"] = song.Song_Skipped,
                    ["offline"] = song.Spotify_Offline,
                    ["offline_timestamp"] = song.Spotify_OfflineTS,
                    ["incognito"] = song.Spotify_Incognito
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending track, '{song.Song_Name}', to JSON: {ex.Message}");
            }
        }).ToList();
    }

    public readonly string?[] GetUriList() => Songs.Where(song => song.Song_URI != null).Select(song => song.Song_URI).ToArray();

    public readonly XDocument GetPlaylist(string name, XNamespace ns)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", ns),
                new XElement(ns + "name", name),
                new XElement(ns + "creator", "SpotifyGPX"),
                new XElement(ns + "trackList",
                    Songs.Select(song =>
                        new XElement(ns + "track",
                            new XElement(ns + "creator", song.Song_Artist),
                            new XElement(ns + "title", song.Song_Name),
                            new XElement(ns + "annotation", song.Time.ToString(Formats.gpxTimeOut)),
                            new XElement(ns + "duration", song.Time_Played)
                        )
                    )
                )
            )
        );
    }
}