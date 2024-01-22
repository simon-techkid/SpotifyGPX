// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX;

public readonly struct Pairings
{
    public Pairings(List<SpotifyEntry> s, List<GPXTrack> t) => PairedPoints = PairPoints(s, t);

    private readonly List<SongPoint> PairedPoints;

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

                Console.WriteLine(pair.ToString());

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
        string countsJoined = string.Join(", ", PairedPoints.GroupBy(pair => pair.Origin).Select(track => $"{track.Count()} ({track.Key.ToString()})"));

        Console.WriteLine($"[PAIR] Paired {PairedPoints.Count} entries: {countsJoined}");
    }

    public readonly void SaveSingleGpx(string path)
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
                PairedPoints.GroupBy(pair => pair.Origin).Select(track =>
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
        Console.WriteLine($"[FILE] '{Path.GetFileName(path)}' - {document.Descendants(Options.OutputNs + "trkpt").Count()}/{PairedPoints.Count()}");
    }

    public readonly void SaveGpxWaypoints(string directory)
    {
        PairedPoints
            .GroupBy(pair => pair.Origin)
            .ToList()
            .ForEach(group =>
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

                string filePath = Path.Combine(directory, $"{group.Key.ToString()}.gpx");
                document.Save(filePath);
                Console.WriteLine($"[FILE] '{Path.GetFileName(filePath)}' - {document.Descendants(Options.OutputNs + "wpt").Count()}/{group.Count()}");
            });
    }

    public readonly void SaveJsonTracks(string directory)
    {
        PairedPoints
            .GroupBy(pair => pair.Origin)
            .ToList()
            .ForEach(group =>
            {
                List<JObject> Json = group.Select(pair => pair.Song.Json).ToList();
                string document = JsonConvert.SerializeObject(Json, Formatting.Indented);
                string filePath = Path.Combine(directory, $"{group.Key.ToString()}.json");
                File.WriteAllText(filePath, document);
                Console.WriteLine($"[FILE] '{Path.GetFileName(filePath)}' - {Json.Count}/{group.Count()}");
            });
    }

    public readonly void SaveUriTracks(string directory)
    {
        PairedPoints
            .GroupBy(pair => pair.Origin)
            .ToList()
            .ForEach(group =>
            {
                string[] strings = group.Select(pair => pair.Song.Song_URI).Where(s => s != null).ToArray();
                string filePath = Path.Combine(directory, $"{group.Key.ToString()}.txt");
                File.WriteAllLines(filePath, strings);
                Console.WriteLine($"[FILE] '{Path.GetFileName(filePath)}' - {strings.Length}/{group.Count()}");
            });
    }

    public readonly void SaveXspfTracks(string directory)
    {
        PairedPoints
            .GroupBy(pair => pair.Origin)
            .ToList()
            .ForEach(group =>
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

                string filePath = Path.Combine(directory, $"{group.Key.ToString()}.xspf");
                document.Save(filePath);
                Console.WriteLine($"[FILE] '{Path.GetFileName(filePath)}' - {document.Descendants(Options.Xspf + "track").Count()}/{group.Count()}");
            });
    }
}