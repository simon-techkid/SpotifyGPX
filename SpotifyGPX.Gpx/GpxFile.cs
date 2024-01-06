// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Gpx;

public readonly struct GpxFile
{
    private readonly XDocument document; // Store the document for on-demand reading
    private static readonly XNamespace Namespace = "http://www.topografix.com/GPX/1/0"; // Default namespace

    public GpxFile(string path)
    {
        try
        {
            // Attempt to load the contents of the specified file into the XML
            document = XDocument.Load(path);
        }
        catch (Exception ex)
        {
            // If the specified XML is invalid, throw an error
            throw new Exception($"The GPX file is incorrectly formatted: {ex.Message}");
        }

        if (!document.Descendants(Namespace + "trk").Any())
        {
            // If there are no <trk> tracks in the GPX, throw error
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        if (!document.Descendants(Namespace + "trkpt").Any())
        {
            // If there are no <trkpt> points the GPX, throw an error
            throw new Exception($"No points found in '{Path.GetFileName(path)}'!");
        }
    }

    private readonly List<XElement> Tracks
    {
        get
        {
            // List all the tracks in the document
            List<XElement> tracks = document.Descendants(Namespace + "trk").ToList();

            // Create track list for user-selected tracks (if the doc contains multiple)
            List<XElement> selected = new();

            // If there are multiple tracks, ask the user which track to use
            if (tracks.Count > 1)
            {
                // If there are multiple <trk> elements, prompt the user to select one

                Console.WriteLine("[TRAK] Multiple GPX tracks found:");
                for (int i = 0; i < tracks.Count; i++)
                {
                    // Print out all the tracks' name values, if they have a name) or an ascending numeric name
                    Console.WriteLine($"[TRAK] [{i}] {tracks[i].Element(Namespace + "name")?.Value ?? $"Track {i}"}");
                }

                Console.WriteLine("[TRAK] [A] All Tracks (Only include songs played during GPX tracking)");
                Console.WriteLine("[TRAK] [B] All Tracks (Include songs played both during & between GPX tracks)");

                Console.Write("[TRAK] Please choose the track you want to use: ");

                // Forever, until the user provides valid input:
                while (true)
                {
                    string? input = Console.ReadLine(); // Read user input

                    // Pair only songs included in the track the user selects:
                    if (int.TryParse(input, out int selectedTrackIndex) && selectedTrackIndex >= 0 && selectedTrackIndex <= tracks.Count)
                    {
                        // Select the user-chosen <trk> element
                        selected.Add(tracks[selectedTrackIndex]);

                        break;
                    }
                    // Pair all songs in the GPX regardless of track:
                    else if (input == "A")
                    {
                        // Return a list of each track separately (they will be discriminable):
                        return tracks;
                    }
                    // Pair all songs in the GPX regardless or track, and regardless of whether they were listened to during GPX journey:
                    else if (input == "B")
                    {
                        // Aggregate all the tracks of the GPX into a single track (they will be cohesive):
                        selected.Add(new XElement("combined", tracks));
                        break;
                    }
                    // User did not provide valid input:
                    else
                    {
                        // Go back to the beginning of the prompt and ask the user again
                        Console.WriteLine("Invalid input. Please enter a valid track number.");
                    }
                }
            }
            // If there is only one track, return the original list (containing one track)
            else
            {
                selected = tracks;
            }

            // Return track(s) in accordance with user selection (if there are multiple) or return the original (if there is one)
            return selected;
        }
    }

    public readonly List<GPXPoint> ParseGpxPoints()
    {
        // Use GroupBy to group <trkpt> elements by their parent <trk> elements.
        var groupedTracks = Tracks
            .SelectMany(trk => trk.Descendants(Namespace + "trkpt") // For each track in the GPX:
                .Select(trkpt => new // For each point in the track:
                {
                    TrackElement = trk, // Set integer based on the parent track
                    Coordinate = new Coordinate( // Create its coordinate
                        double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")), // Lat
                        double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX")) // Lon
                    ),
                    Time = trkpt.Element(Namespace + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX") // Create its time
                }))
            .GroupBy(data => data.TrackElement); // Return grouped tracks containing parsed points

        // Start at track zero
        int trkInteger = 0;

        // Parse GPXPoint from each point
        return groupedTracks
            .SelectMany(group => // For each track
            {
                XElement trk = group.Key; // Get the track's original XElement
                List<GPXPoint> trkPoints = group // Create List<GPXPoint> of the track's points
                    .Select((pointData, index) => new GPXPoint( // For each point in the track, parse:
                        pointData.Coordinate, // Lat/Lon
                        pointData.Time,       // Time
                        trkInteger,           // Track Member
                        index                 // Index
                    ))
                    .ToList(); // Export the List<GPXPoint> of points contained in this track

                // After this track is completely parsed, add one to the identifier so that the next track's points is distinguishable
                trkInteger++;

                // Return this track's points
                return trkPoints;
            })
            .ToList(); // Return List<GPXPoint> containing all the parsed points
    }
}
