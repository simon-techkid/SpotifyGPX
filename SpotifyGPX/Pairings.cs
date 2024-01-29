// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX;

public class Pairings
{
    private readonly List<SongPoint> Pairs;

    public Pairings(List<SpotifyEntry> s, List<GPXTrack> t) => Pairs = PairPoints(s, t);

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXTrack> gpxTracks)
    {
        // Correlate Spotify entries with the nearest GPX points

        int index = 0; // Index of the pairing

        List<SongPoint> correlatedEntries = gpxTracks // For each GPX track
        .SelectMany(gpxTrack => songs // Get the list of SpotifyEntries
        .Where(spotifyEntry => spotifyEntry.WithinTimeFrame(gpxTrack.Start, gpxTrack.End)) // If the SpotifyEntry falls within the boundaries of the track
        .Select(spotifyEntry => // Select the Spotify entry if it falls in range of the GPX track
            {
                SongPoint pair = gpxTrack.Points
                .Select(point => new SongPoint(index, spotifyEntry, point, gpxTrack.Track)) // For each point in the track's point list,
                .OrderBy(pair => pair.AbsAccuracy) // Order the points by proximity between point and song
                .First(); // Closest accuracy wins

                Console.WriteLine(pair.ToString());

                index++; // Add to the index of all pairings regardless of track

                return pair;
            })
        )
        .Where(pair => Options.MaximumAbsAccuracy == null || pair.AbsAccuracy <= Options.MaximumAbsAccuracy) // Only create pairings with accuracy equal to or below max allowed accuracy
        .ToList();

        if (correlatedEntries.Count > 0)
        {
            // Calculate and print the average correlation accuracy in seconds
            Console.WriteLine($"[PAIR] Song-Point Correlation Accuracy (avg sec): {Math.Round(correlatedEntries.Average(correlatedPair => correlatedPair.AbsAccuracy))}");
        }

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }

    public void PrintTracks()
    {
        string countsJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin).Select(track => $"{track.Count()} ({track.Key.ToString()})"));

        Console.WriteLine($"[PAIR] Paired {Pairs.Count} entries from {Pairs.GroupBy(pair => pair.Origin).Count()} tracks: {countsJoined}");
    }

    public void SaveSingleGpx(string path)
    {
        XDocument document = new(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Options.OutputNs + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Options.Xsi),
                new XAttribute("xmlns", Options.OutputNs),
                new XAttribute(Options.Xsi + "schemaLocation", Options.Schema),
                new XElement(Options.OutputNs + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.GpxOutput)),
                Pairs.GroupBy(pair => pair.Origin).Select(track =>
                    new XElement(Options.OutputNs + "trk",
                        new XElement(Options.OutputNs + "name", track.Key.ToString()),
                        new XElement(Options.OutputNs + "trkseg",
                            track.Select(pair =>
                                new XElement(Options.OutputNs + "trkpt",
                                    new XAttribute("lat", pair.Point.Location.Latitude),
                                    new XAttribute("lon", pair.Point.Location.Longitude),
                                    new XElement(Options.OutputNs + "name", pair.Song.ToString()),
                                    new XElement(Options.OutputNs + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
                                    new XElement(Options.OutputNs + "desc", pair.Description)
                                )
                            )
                        )
                    )
                )
            )
        );

        document.Save(path);
        Console.WriteLine($"[FILE] {document.Descendants(Options.OutputNs + "trkpt").Count()} points ==> {path}");
    }

    public void SaveGpxWaypoints(string prefix, string directory, string suffix)
    {
        List<string> results = Pairs
            .GroupBy(pair => pair.Origin)
            .Select(group =>
            {
                XDocument document = new(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(Options.OutputNs + "gpx",
                        new XAttribute("version", "1.0"),
                        new XAttribute("creator", "SpotifyGPX"),
                        new XAttribute(XNamespace.Xmlns + "xsi", Options.Xsi),
                        new XAttribute("xmlns", Options.OutputNs),
                        new XAttribute(Options.Xsi + "schemaLocation", Options.Schema),
                        new XElement(Options.OutputNs + "name", group.Key.ToString()),
                        new XElement(Options.OutputNs + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.GpxOutput)),
                        group.Select(pair =>
                            new XElement(Options.OutputNs + "wpt",
                                new XAttribute("lat", pair.Point.Location.Latitude),
                                new XAttribute("lon", pair.Point.Location.Longitude),
                                new XElement(Options.OutputNs + "name", pair.Song.ToString()),
                                new XElement(Options.OutputNs + "time", pair.Point.Time.UtcDateTime.ToString(Options.GpxOutput)),
                                new XElement(Options.OutputNs + "desc", pair.Description)
                            )
                        )
                    )
                );

                string filePath = Path.Combine(directory, $"{prefix}_{group.Key.ToString()}_{suffix}.gpx");
                document.Save(filePath);

                return TrackCountsString(document.Descendants(Options.OutputNs + "wpt").Count(), group.Key);
            })
            .ToList();

        Console.WriteLine(SaveFileString("GPX", results));
    }

    public void SaveJsonTracks(string prefix, string directory)
    {
        List<string> results = Pairs
            .GroupBy(pair => pair.Origin)
            .Select(group =>
            {
                List<JObject> Json = group.Select(pair => pair.Song.Json).ToList();
                string document = JsonConvert.SerializeObject(Json, Options.Json);

                string filePath = Path.Combine(directory, $"{prefix}_{group.Key.ToString()}.json");
                File.WriteAllText(filePath, document);

                return TrackCountsString(Json.Count, group.Key);
            })
            .ToList();

        Console.WriteLine(SaveFileString("JSON", results));
    }

    public void SaveUriTracks(string prefix, string directory)
    {
        List<string> results = Pairs
            .GroupBy(pair => pair.Origin)
            .Select(group =>
            {
                string[] strings = group.Select(pair => pair.Song.Song_URI).Where(s => s != null).ToArray();

                string filePath = Path.Combine(directory, $"{prefix}_{group.Key.ToString()}.txt");
                File.WriteAllLines(filePath, strings);

                return TrackCountsString(strings.Length, group.Key);
            })
            .ToList();

        Console.WriteLine(SaveFileString("TXT", results));
    }

    public void SaveXspfTracks(string prefix, string directory)
    {
        List<string> results = Pairs
            .GroupBy(pair => pair.Origin)
            .Select(group =>
            {
                XDocument document = new(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(Options.Xspf + "playlist",
                    new XAttribute("version", "1.0"),
                    new XAttribute("xmlns", Options.Xspf),
                    new XElement(Options.Xspf + "name", group.Key.ToString()),
                    new XElement(Options.Xspf + "creator", "SpotifyGPX"),
                    new XElement(Options.Xspf + "trackList",
                        group.Select(song =>
                        new XElement(Options.Xspf + "track",
                            new XElement(Options.Xspf + "creator", song.Song.Song_Artist),
                            new XElement(Options.Xspf + "title", song.Song.Song_Name),
                            new XElement(Options.Xspf + "annotation", song.Song.Time.UtcDateTime.ToString(Options.GpxOutput)),
                            new XElement(Options.Xspf + "duration", song.Song.Time_Played)
                            )
                        )
                    )
                )
                );

                string filePath = Path.Combine(directory, $"{prefix}_{group.Key.ToString()}.xspf");
                document.Save(filePath);

                return TrackCountsString(document.Descendants(Options.Xspf + "track").Count(), group.Key);
            })
            .ToList();

        Console.WriteLine(SaveFileString("XSPF", results));
    }

    private static string TrackCountsString(int count, TrackInfo track) => $"{count} ({track.ToString()})";

    private string SaveFileString(string filetype, List<string> payload) => $"[FILE] {Pairs.Count} points ==> {payload.Count} {filetype}s: {string.Join(", ", payload)}";
}