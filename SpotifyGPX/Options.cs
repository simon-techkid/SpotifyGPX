// SpotifyGPX by Simon Field

using System.Globalization;

namespace SpotifyGPX;

public class Options
{
    // Time Formats for Console Printing
    public static string Console => @"HH\:mm\:ss"; // For each pairing printed to console (point & song time printed this way)
    public static string ConsoleTrack => @"yyyy-MM-dd HH\:mm\:ss zzz"; // Each track's (if there are multiple) start and end time

    // GPX Import Formats
    public static string GpxInput => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz"; // Time format of a <time> element of a <trkpt> within input GPX
    public static DateTimeStyles GpxTimeStyle => DateTimeStyles.None; // Time zone interpretation of GPX points

    // JSON Import Formats
    public static string SpotifyFull => @"MM/dd/yyyy HH\:mm\:ss"; // 30 day (full acc data) dump
    public static string SpotifyMini => @"yyyy-MM-dd HH\:mm"; // 5 day (past year) dump
    public static DateTimeStyles SpotifyTimeStyle => DateTimeStyles.AssumeUniversal; // Time zone interpretation of Spotify data

    // Time Output Formats
    public static string GpxOutput => @"yyyy-MM-ddTHH\:mm\:ssZ"; // Must first be converted to UTC (GPX spec requires ISO 8601)
    public static string DescriptionPlayedAt => @"yyyy-MM-dd HH\:mm\:ss zzz"; // Time of a song and point printed in the <desc> field of exported GPX
    public static string DescriptionTimePlayed => @"hh\:mm\:ss\.fff"; // Duration (msPlayed) of a song in the <desc> field of exported GPX
}

