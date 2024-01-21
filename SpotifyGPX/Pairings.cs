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

        int index = 0; // Index of the pairing

        List<SongPoint> correlatedEntries = tracks // For each GPX track
        .SelectMany(track => songs // Get the list of SpotifyEntries
        .Where(spotifyEntry => (spotifyEntry.Time >= track.Start) && (spotifyEntry.Time <= track.End)) // If the SpotifyEntry falls within the boundaries of the track
        .Select(spotifyEntry => // Select the Spotify entry if it falls in range of the GPX track
            {
                SongPoint pair = track.Points
                .Select(point => new SongPoint(index, spotifyEntry, point, track.Track)) // For each point in the track's point list,
                .OrderBy(pair => pair.AbsAccuracy) // Order the points by proximity between point and song
                .First(); // Closest accuracy wins

                Console.WriteLine(pair);

                index++; // Add to the index of all pairings regardless of track

                return pair;
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

        Console.WriteLine($"[PAIR] Paired {PairedPoints.Count} entries: {countsJoined}");
    }

    public readonly XDocument GetGpx(string name)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Options.OutputNs + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Options.Xsi),
                new XAttribute("xmlns", Options.OutputNs),
                new XAttribute(Options.Xsi + "schemaLocation", Options.Schema),
                new XElement(Options.OutputNs + "name", name),
                new XElement(Options.OutputNs + "time", DateTime.Now.ToUniversalTime().ToString(Options.GpxOutput)),
                PairedPoints.GroupBy(pair => pair.Origin).Select(track =>
                    new XElement(Options.OutputNs + "trk",
                        new XElement(Options.OutputNs + "name", track.Key.Name),
                        new XElement(Options.OutputNs + "trkseg",
                            track.Select(pair =>
                                new XElement(Options.OutputNs + "trkpt",
                                    new XAttribute("lat", pair.Point.Location.Latitude),
                                    new XAttribute("lon", pair.Point.Location.Longitude),
                                    new XElement(Options.OutputNs + "name", pair.Song),
                                    new XElement(Options.OutputNs + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
                                    new XElement(Options.OutputNs + "desc", pair.Description)
                                )
                            )
                        )
                    )
                )
            )
        );
    }

    /*
    public readonly XDocument GetGpxx(string name)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Options.OutputNs + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Options.Xsi),
                new XAttribute("xmlns", Options.OutputNs),
                new XAttribute(Options.Xsi + "schemaLocation", Options.Schema),
                new XElement(Options.OutputNs + "name", name),
                new XElement(Options.OutputNs + "time", DateTime.Now.ToUniversalTime().ToString(Options.GpxOutput)),
                PairedPoints.Select(pair =>
                    new XElement(Options.OutputNs + "wpt",
                        new XAttribute("lat", pair.Point.Location.Latitude),
                        new XAttribute("lon", pair.Point.Location.Longitude),
                        new XElement(Options.OutputNs + "name", pair.Song),
                        new XElement(Options.OutputNs + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
                        new XElement(Options.OutputNs + "desc", pair.Description)
                    )
                )
            )
        );
    }
    */

    public readonly List<JObject> GetJson() => Songs.Select(song => song.Json).ToList();

    public readonly string?[] GetUriList() => Songs.Where(song => song.Song_URI != null).Select(song => song.Song_URI).ToArray();

    public readonly XDocument GetPlaylist(string name)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Options.Xspf + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Options.Xspf),
                new XElement(Options.Xspf + "name", name),
                new XElement(Options.Xspf + "creator", "SpotifyGPX"),
                new XElement(Options.Xspf + "trackList",
                    Songs.Select(song =>
                        new XElement(Options.Xspf + "track",
                            new XElement(Options.Xspf + "creator", song.Song_Artist),
                            new XElement(Options.Xspf + "title", song.Song_Name),
                            new XElement(Options.Xspf + "annotation", song.Time.UtcDateTime.ToString(Options.GpxOutput)),
                            new XElement(Options.Xspf + "duration", song.Time_Played)
                        )
                    )
                )
            )
        );
    }
}