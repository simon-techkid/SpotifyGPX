// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace SpotifyGPX;

public struct Options
{
    // Time Formats for Console Printing
    public static string Console => @"HH\:mm\:ss"; // For each pairing printed to console (point & song time printed this way)
    public static string ConsoleTrack => @"yyyy-MM-dd HH\:mm\:ss zzz"; // Each track's (if there are multiple) start and end time

    // GPX Import Formats
    public static string GpxInput => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz"; // Time format of a <time> element of a <trkpt> within input GPX
    public static DateTimeStyles GpxTimeStyle => DateTimeStyles.None; // Time zone interpretation of GPX points
    public static XNamespace InputNs => "http://www.topografix.com/GPX/1/0"; // Namespace of input GPX

    // JSON Import Formats
    public static string SpotifyFull => @"MM/dd/yyyy HH\:mm\:ss"; // 30 day (full acc data) dump
    public static string SpotifyMini => @"yyyy-MM-dd HH\:mm"; // 5 day (past year) dump
    public static DateTimeStyles SpotifyTimeStyle => DateTimeStyles.AssumeUniversal; // Time zone interpretation of Spotify data

    // Song Filtering Options
    public static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum playback time of song for it to be considered (songs played equal to or longer than this timespan will be considered)
    public static bool ExcludeSkipped => false; // Include or exclude songs that were skipped by the user, as defined by Spotify JSON

    // GPX Export Formats
    public static string GpxOutput => @"yyyy-MM-ddTHH\:mm\:ssZ"; // Must first be converted to UTC (GPX spec requires ISO 8601)
    public static string DescriptionPlayedAt => @"yyyy-MM-dd HH\:mm\:ss zzz"; // Time of a song and point printed in the <desc> field of exported GPX
    public static string DescriptionTimePlayed => @"hh\:mm\:ss\.fff"; // Duration (msPlayed) of a song in the <desc> field of exported GPX
    public static XNamespace OutputNs => "http://www.topografix.com/GPX/1/0"; // Namespace of the output GPX
    public static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance"; // XML schema location of the output GPX
    public static string Schema => "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd"; // GPX schema location(s) of the output GPX

    // XSPF Export Options
    public static XNamespace Xspf => "http://xspf.org/ns/0/"; // Namespace of output XSPF

    // JSON Export Options
    public static Formatting Json => Formatting.Indented; // Formatting of exporting JSON
}

