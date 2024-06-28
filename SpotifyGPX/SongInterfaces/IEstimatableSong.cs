// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using System;

namespace SpotifyGPX.SongInterfaces;

/// <summary>
/// Provides access to estimated start/end times for a song.
/// </summary>
public interface IEstimatableSong : IDuratableSong
{
    /// <summary>
    /// Determines whether to treat <see cref="Time"/> as the <see cref="TimeInterpretation.Start"/> (start)
    /// or the <see cref="TimeInterpretation.End"/> (end) time of a song.
    /// </summary>
    public TimeInterpretation CurrentInterpretation { get; }

    /// <summary>
    /// The date and time provided by the given <see cref="ISongInput"/> file.
    /// Can be the start or the end time of the song, as determined by <see cref="TimeInterpretation"/>.
    /// </summary>
    public DateTimeOffset FriendlyTime { get; }

    /// <summary>
    /// The duration of time during which the song was played.
    /// This value is required for estimation as it provides a basis for calculating the end or start time of the song.
    /// </summary>
    public new TimeSpan TimePlayed { get; }

    /// <summary>
    /// The estimated or exact start time of this song.
    /// If <see cref="TimeInterpretation"/> is <see cref="TimeInterpretation.Start"/>, this value is exact.
    /// If <see cref="TimeInterpretation"/> is <see cref="TimeInterpretation.End"/>, this value is estimated based on <see cref="TimePlayed"/>
    /// </summary>
    public DateTimeOffset TimeStarted
    {
        get
        {
            return CurrentInterpretation switch
            {
                TimeInterpretation.Start => FriendlyTime,
                TimeInterpretation.End => FriendlyTime - TimePlayed,
                _ => throw new InvalidOperationException("Time interpretation not set.")
            };

        }
    }

    /// <summary>
    /// The estimated or exact end time of this song.
    /// If <see cref="TimeInterpretation"/> is <see cref="TimeInterpretation.Start"/>, this value is estimated based on <see cref="TimePlayed"/>.
    /// If <see cref="TimeInterpretation"/> is <see cref="TimeInterpretation.End"/>, this value is exact.
    /// </summary>
    public DateTimeOffset TimeEnded
    {
        get
        {
            return CurrentInterpretation switch
            {
                TimeInterpretation.Start => FriendlyTime + TimePlayed,
                TimeInterpretation.End => FriendlyTime,
                _ => throw new InvalidOperationException("Time interpretation not set.")
            };
        }
    }
}
