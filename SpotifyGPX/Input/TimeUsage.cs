namespace SpotifyGPX.Input;

/// <summary>
/// Instructs the parser to treat the song's primary time as the start or end of the song.
/// </summary>
public enum TimeUsage
{
    /// <summary>
    /// Treat the song's time as the point at which the song began.
    /// If the song's start time is not provided by the format,
    /// estimate it based on the duration of playback and the end time.
    /// </summary>
    Start,

    /// <summary>
    /// Treat the song's time as the point at which the song ended.
    /// If the song's end time is not provided by the format,
    /// estimate it based on the start time and the duration of playback.
    /// </summary>
    End
}
