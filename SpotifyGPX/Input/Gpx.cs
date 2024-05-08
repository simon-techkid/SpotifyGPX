// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public sealed partial class Gpx : GpsInputBase
{
    private XDocument Document { get; set; }
    protected override ParseTracksDelegate ParseTracksMethod => ParseTracks;

    public Gpx(string path) : base(path)
    {
        Document = XDocument.Load(StreamReader, loadOptions);
    }

    public override int SourceTrackCount => Document.Descendants(InputNs + Track).Count();

    public override int SourcePointCount => Document.Descendants(InputNs + TrackPoint).Count();

    private List<GpsTrack> ParseTracks()
    {
        return Document.Descendants(InputNs + Track)
            .Select((trk, index) => new GpsTrack( // For each track and its index, create a new GPXTrack
                index,
                trk.Element(InputNs + "name")?.Value,
                TrackType.GPX,
                trk.Descendants(InputNs + TrackPoint)
                    .Select((trkpt, pointIndex) => (IGpsPoint)new GpxPoint
                    {
                        Index = pointIndex,
                        Location = new Coordinate( // Parse its coordinate
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        Time = DateTimeOffset.ParseExact(trkpt.Element(InputNs + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX"), TimeFormat, null, TimeStyle)
                    }).ToList() // Send all points to List<GPXPoint>
            ))
            .ToList(); // Send all tracks to List<GPXTrack>
    }

    protected override void ClearDocument()
    {
        Document.Root?.RemoveAll();
    }
}
