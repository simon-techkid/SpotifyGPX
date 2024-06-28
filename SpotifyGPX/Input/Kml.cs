// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using SpotifyGPX.PointEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public sealed partial class Kml : GpsInputBase
{
    private XDocument Document { get; }
    protected override string FormatName => nameof(Kml);
    public override List<GpsTrack> ParseTracksMethod() => ParseTracks();
    public override List<GpsTrack> FilterTracksMethod() => FilterTracks();

    public Kml(string path, StringBroadcaster bcast) : base(path, bcast)
    {
        Document = XDocument.Load(StreamReader, loadOptions);
    }

    private List<GpsTrack> ParseTracks()
    {
        return Document
            .Descendants(Gx + "Track") // Find all the tracks in the KML
            .Select(track => // For each track,
            {
                var coords = track.Elements(Gx + "coord"); // Get the coordinates of all the track's points

                List<IGpsPoint> points = track
                .Elements(InputNs + "when") // Get the timestamps of all the track's points
                .Select((when, index) => // Select each time:
                {
                    string timestr = when.Value; // Get the time string
                    DateTimeOffset time = DateTimeOffset.ParseExact(timestr, TimeFormat, null, TimeStyle); // Convert the time string to a DateTimeOffset

                    string[] coord = coords.ElementAt(index).Value.Split(' '); // Get the corresponding coordinate string
                    double longitude = double.Parse(coord[0]);
                    double latitude = double.Parse(coord[1]);
                    double altitude = double.Parse(coord[2]);

                    return (IGpsPoint)new KmlPoint(index, new Coordinate(latitude, longitude), time);

                }).ToList();

                return new GpsTrack(null, null, TrackType.Gps, points);

            }).ToList();
    }

    private List<GpsTrack> FilterTracks()
    {
        return AllTracks.Where(track => track.OfType<KmlPoint>().All(point => filter(point))).ToList();
    }

    protected override void DisposeDocument()
    {
        Document.Root?.RemoveAll();
    }

    public override int SourceTrackCount => Document.Descendants(Gx + "Track").Count();

    public override int SourcePointCount => Document.Descendants(Gx + "coord").Count();
}
