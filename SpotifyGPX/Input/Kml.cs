// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing GPS data from the KML format.
/// </summary>
public partial class Kml : GpsInputBase
{
    private XDocument Document { get; }
    protected override List<GPXTrack> Tracks { get; }

    /// <summary>
    /// Creates a new input handler for handling files in the KML format.
    /// </summary>
    /// <param name="path">The path to the KML file.</param>
    public Kml(string path)
    {
        Document = XDocument.Load(path);

        Tracks = ParseTracks();
    }

    /// <summary>
    /// The total number of tracks in the source KML file.
    /// </summary>
    public override int SourceTrackCount => Document.Descendants(Gx + "Track").Count();

    /// <summary>
    /// The total number of points in the source KML file.
    /// </summary>
    public override int SourcePointCount => Document.Descendants(Gx + "coord").Count();

    /// <summary>
    /// Parses this KML document into a readable list of tracks.
    /// </summary>
    /// <returns>A list of GPXTrack objects.</returns>
    private List<GPXTrack> ParseTracks()
    {
        return Document
            .Descendants(Gx + "Track") // Find all the tracks in the KML
            .Select(track => // For each track,
            {
                var coords = track.Elements(Gx + "coord"); // Get the coordinates of all the track's points

                List<GPXPoint> points = track
                .Elements(InputNs + "when") // Get the timestamps of all the track's points
                .Select((when, index) => // Select each time:
                {
                    string timestr = when.Value; // Get the time string
                    DateTimeOffset time = DateTimeOffset.ParseExact(timestr, TimeFormat, null, TimeStyle); // Convert the time string to a DateTimeOffset

                    string[] coord = coords.ElementAt(index).Value.Split(' '); // Get the corresponding coordinate string
                    double longitude = double.Parse(coord[0]);
                    double latitude = double.Parse(coord[1]);
                    double altitude = double.Parse(coord[2]);

                    return new GPXPoint(index, new Coordinate(latitude, longitude), time);

                }).ToList();

                return new GPXTrack(null, null, TrackType.GPX, points);

            }).ToList();
    }
}
