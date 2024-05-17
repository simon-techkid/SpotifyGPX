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

    public abstract IGpsInput.FilterTracksDelegate FilterTracksMethod { get; }

    public abstract IGpsInput.ParseTracksDelegate ParseTracksMethod { get; }

    /// <summary>
    /// Access all tracks in this GPS data file.
    /// </summary>
    protected List<GpsTrack> AllTracks => ParseTracksMethod();

    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => AllTracks.Count;
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => AllTracks.Select(track => track.Points.Count).Sum();
}
