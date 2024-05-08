// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of GPS journey files. All classes that handle GPS journey files must inherit this class.
/// </summary>
public abstract class GpsInputBase : FileInputBase, IGpsInput
{
    protected GpsInputBase(string path) : base(path)
    {
    }

    protected delegate List<GpsTrack> ParseTracksDelegate();
    protected abstract ParseTracksDelegate ParseTracksMethod { get; }
    protected List<GpsTrack> Tracks => ParseTracksMethod();
    public List<GpsTrack> GetAllTracks() => Tracks;
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => Tracks.Count;
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => Tracks.Select(track => track.Points.Count).Sum();
}
