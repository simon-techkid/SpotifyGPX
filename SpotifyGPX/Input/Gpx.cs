// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public sealed partial class Gpx : GpsInputBase
{
    private XDocument Document { get; set; }
    public override List<GpsTrack> ParseTracksMethod() => ParseTracks();
    public override List<GpsTrack> FilterTracksMethod() => FilterTracks();

    public Gpx(string path) : base(path)
    {
        Document = XDocument.Load(StreamReader, loadOptions);
    }

    private List<GpsTrack> ParseTracks()
    {
        return Document.Descendants(InputNs + Track)
            .Select((trk, index) => new GpsTrack( // For each track and its index, create a new GpsTrack
                index,
                trk.Element(InputNs + "name")?.Value,
                TrackType.Gps,
                trk.Descendants(InputNs + TrackPoint)
                    .Select((trkpt, pointIndex) => (IGpsPoint)new GpxPoint
                    {
                        Index = pointIndex,
                        Location = new Coordinate( // Parse its coordinate
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        Time = DateTimeOffset.ParseExact(trkpt.Element(InputNs + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX"), TimeFormat, null, TimeStyle)
                    }).ToList() // Send all points to list
            ))
            .ToList(); // Send all tracks to list
    }

    private List<GpsTrack> FilterTracks()
    {
        return AllTracks.Where(track => track.OfType<GpxPoint>().All(point => filter(point))).ToList();
    }

    protected override void DisposeDocument()
    {
        Document.Root?.RemoveAll();
    }

    public override int SourceTrackCount => Document.Descendants(InputNs + Track).Count();

    public override int SourcePointCount => Document.Descendants(InputNs + TrackPoint).Count();
}
