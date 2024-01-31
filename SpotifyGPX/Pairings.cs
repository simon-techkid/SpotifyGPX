// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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

    public enum CohesiveFormat
    {
        GPX,
        JsonReport
    }

    public void SaveCohesive(CohesiveFormat format, string path)
    {
        switch (format)
        {
            case CohesiveFormat.GPX:
                XDocument document = GpxTracks();
                document.Save(path);
                return;

            case CohesiveFormat.JsonReport:
                List<JObject> report = JsonReport();
                string rep = JsonConvert.SerializeObject(report, Options.Json);
                File.WriteAllText(path, rep);
                return;

            default:
                throw new ArgumentException("Invalid output format");
        }
    }

    private XDocument GpxTracks()
    {
        IEnumerable<XElement> ele = Pairs
            .GroupBy(pair => pair.Origin)
            .Select(track =>
            {
                return new XElement(Options.OutputNs + "trk",
                    new XElement(Options.OutputNs + "name", track.Key.ToString()),
                    new XElement(Options.OutputNs + "trkseg",
                        track.Select(pair => pair.ToGPX("trkpt"))
                    )
                );
            });

        return GetGpx(ele);
    }

    private List<JObject> JsonReport()
    {
        return Pairs
            .GroupBy(pair => pair.Origin)
            .Select(group =>
            {
                return new JObject(
                    new JProperty(group.Key.ToString(), group
                    .SelectMany(pair =>
                    {
                        return new JArray(pair.CreateJsonReport());
                    }))
                );
            })
            .ToList();
    }

    public enum TrackFormat
    {
        GPX,
        JSON,
        TXT,
        XSPF
    }

    public void SaveTracks(TrackFormat format, string? prefix, string directory, string? suffix)
    {
        List<string> results = Pairs // For all pairs:
            .GroupBy(pair => pair.Origin) // Group them by origin track
            .Select(group => // For each group of pairs (for each track):
            {
                TrackInfo currentTrack = group.Key; // Send the info of the current track to a variable

                string fileNameWithoutExtension = string.Join("_", new[] { prefix, currentTrack.ToString(), suffix }.Where(component => !string.IsNullOrEmpty(component))); // if prefix or suffix null, don't include
                string extension = format.ToString().ToLower(); // file extension is selected format in lowercase letters
                string fileNameWithExtension = $"{fileNameWithoutExtension}.{extension}"; // pair the filename and file extension

                string filePath = Path.Combine(directory, fileNameWithExtension); // Create full file path that has the parent directory before above file name

                switch (format)
                {
                    case TrackFormat.GPX:
                        XDocument gpxDocument = GpxWaypoints(group); // generate a GPX file
                        Save(gpxDocument, filePath); // save that file
                        return TrackCountsString(gpxDocument.Descendants(Options.OutputNs + "wpt").Count(), currentTrack); // return the per-track point counts

                    case TrackFormat.JSON:
                        List<JObject> jsonList = JsonObjects(group); // generate a JSON file
                        string json = JsonConvert.SerializeObject(jsonList, Options.Json); // Combine objects into a string
                        Save(json, filePath); // save that file
                        return TrackCountsString(jsonList.Count, currentTrack); // return the per-track song counts

                    case TrackFormat.TXT:
                        string[] txtArray = UriList(group); // generate a TXT file
                        Save(txtArray, filePath); // save that file
                        return TrackCountsString(txtArray.Length, currentTrack); // return the per-track song uri counts

                    case TrackFormat.XSPF:
                        XDocument xspfDocument = GetXspf(group); // generate an XSPF file
                        Save(xspfDocument, filePath); // save that file
                        return TrackCountsString(xspfDocument.Descendants(Options.Xspf + "track").Count(), currentTrack); // return the per-track song counts

                    default:
                        throw new ArgumentException("Invalid output format");
                }
            })
            .ToList();

        Console.WriteLine(SaveFileString(format, results));
    }

    private static string TrackCountsString(int count, TrackInfo track) => $"{count} ({track.ToString()})";

    private string SaveFileString(TrackFormat filetype, List<string> payload) => $"[FILE] {Pairs.Count} points ==> {payload.Count} {filetype}s: {string.Join(", ", payload)}";

    private static void Save(object content, string path)
    {
        switch (content)
        {
            case XDocument xDoc:
                xDoc.Save(path);
                break;
            case string json:
                File.WriteAllText(path, json);
                break;
            case string[] array:
                File.WriteAllLines(path, array);
                break;
            default:
                throw new Exception($"Unable to save pairings document to {path}");
        }
    }

    private static XDocument GpxWaypoints(IEnumerable<SongPoint> pairs)
    {
        return GetGpx(pairs.Select(pair => pair.ToGPX("wpt")));
    }

    private static List<JObject> JsonObjects(IEnumerable<SongPoint> pairs) => pairs.Select(pair => pair.Song.Json).ToList();

    private static string[] UriList(IEnumerable<SongPoint> pairs) => pairs.Select(pair => pair.Song.Song_URI).Where(s => s != null).ToArray();

    private static XDocument GetGpx(IEnumerable<XElement> ele)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Options.OutputNs + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Options.Xsi),
                new XAttribute("xmlns", Options.OutputNs),
                new XAttribute(Options.Xsi + "schemaLocation", Options.Schema),
                new XElement(Options.OutputNs + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.GpxOutput)),
                ele
            )
        );
    }

    private static XDocument GetXspf(IEnumerable<SongPoint> pairs)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Options.Xspf + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Options.Xspf),
                new XElement(Options.Xspf + "creator", "SpotifyGPX"),
                new XElement(Options.Xspf + "trackList",
                    pairs.Select(song => song.Song.ToXspf())
                )
            )
        );
    }

    public override string ToString()
    {
        string countsJoined = string.Join(", ", Pairs.GroupBy(pair => pair.Origin).Select(track => $"{track.Count()} ({track.Key.ToString()})"));

        return $"[PAIR] Paired {Pairs.Count} entries from {Pairs.GroupBy(pair => pair.Origin).Count()} tracks: {countsJoined}";
    }
}