namespace SpotifyGPX.Input;

/// <summary>
/// Instructs the parser to interpret a time as the start or end of a song.
/// </summary>
public enum TimeInterpretation
{
    /// <summary>
    /// Interpret this time as the time at which the song's playback began.
    /// </summary>
    Start,

    /// <summary>
    /// Interpret this time as the time at which the song's playback ended.
    /// </summary>
    End
}
