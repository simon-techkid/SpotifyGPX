// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System.Globalization;

namespace SpotifyGPX;

public class Options
{
    // Console Printing

    /// <summary>
    /// The format in which pair times will be printed to the console.
    /// </summary>
    public static string Console => @"HH\:mm\:ss";

    /// <summary>
    /// The format in which tracks (start and end times) will be printed to the console.
    /// </summary>
    public static string ConsoleTrack => @"yyyy-MM-dd HH\:mm\:ss zzz";

    // GPX

    /// <summary>
    /// A format string given to construct a GPXPoint's DateTimeOffset.
    /// </summary>
    public static string GpxInput => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz";

    /// <summary>
    /// Time zone interpretation of constructed GPXPoint objects.
    /// </summary>
    public static DateTimeStyles GpxTimeStyle => DateTimeStyles.None;

    // JSON

    /// <summary>
    /// The format of Spotify's 30 day (full acc data) dump JSON.
    /// </summary>
    public static string SpotifyFull => @"yyyy-MM-ddTHH\:mm\:ssZ";

    /// <summary>
    /// The format of Spotify's 5 day (past year) dump JSON.
    /// </summary>
    public static string SpotifyMini => @"yyyy-MM-dd HH\:mm";

    /// <summary>
    /// Time zone interpretation of constructed SpotifyEntry objects.
    /// </summary>
    public static DateTimeStyles SpotifyTimeStyle => DateTimeStyles.AssumeUniversal;

    /// <summary>
    /// Allow SpotifyGPX to use the estimated start time instead of the known end time.
    /// Estimated by subtracting the time played from the end time.
    /// This assumes the user didn't pause playback.
    /// </summary>
    public static bool PreferEstimatedStartTime => true;

    // Time Outputs

    /// <summary>
    /// The format in which DateTimeOffset objects will be written to XML time fields.
    /// DateTimeOffset must be converted to UTC before converting to string.
    /// </summary>
    public static string GpxOutput => @"yyyy-MM-ddTHH\:mm\:ssZ";

    /// <summary>
    /// The format in which DateTimeOffset objects will be written to user-visible XML description fields.
    /// Can be any UTC offset.
    /// </summary>
    public static string DescriptionPlayedAt => @"yyyy-MM-dd HH\:mm\:ss zzz";

    /// <summary>
    /// The format in which TimeSpan objects will be written to user-visible XML descriptio fields.
    /// </summary>
    public static string DescriptionTimePlayed => @"hh\:mm\:ss\.fff";

    /// <summary>
    /// JSON settings requiring verbatim time handling.
    /// </summary>
    public static JsonSerializerSettings JsonSettings => new()
    {
        DateParseHandling = DateParseHandling.None,
        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
    };
}

