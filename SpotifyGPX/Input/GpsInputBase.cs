// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of GPS journey files. All classes that handle GPS journey files must inherit this class.
/// </summary>
public abstract class GpsInputBase : GpsInputSelection, IGpsInput
{
    protected GpsInputBase(string path) : base(path)
    {
    }

    /// <summary>
    /// A delegate providing access to all tracks within this GPS input file class.
    /// </summary>
    /// <returns>A list of <see cref="GpsTrack"/> objects, each <see cref="GpsTrack"/> representing a series of GPS points (<see cref="IGpsPoint"/>).</returns>
    protected delegate List<GpsTrack> ParseTracksDelegate();

    /// <summary>
    /// Provides access to all tracks within this GPS input file.
    /// </summary>
    protected abstract ParseTracksDelegate ParseTracksMethod { get; }

    /// <summary>
    /// Access all tracks in this GPS data file.
    /// </summary>
    protected virtual List<GpsTrack> Tracks => ParseTracksMethod();

    public override List<GpsTrack> GetAllTracks() => Tracks;
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => Tracks.Count;
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => Tracks.Select(track => track.Points.Count).Sum();
}
