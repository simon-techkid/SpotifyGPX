// SpotifyGPX by Simon Field

using System.Globalization;
using System.Xml.Linq;

namespace SpotifyGPX;

public struct Options
{
    // Time format for console printing of point-song time comparison:
    public static string Console => @"HH\:mm\:ss";
    public static string ConsoleTrack => @"yyyy-MM-dd HH\:mm\:ss zzz"; // when there are multiple tracks

    // ================== //
    // GPX IMPORT FORMATS //
    // ================== //

    // Time format used to interpret your GPX track <time> tags
    public static string GpxInput => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz"; // Can be any UTC offset
    public static XNamespace InputNs => "http://www.topografix.com/GPX/1/0"; // Namespace of the input GPX

    // =================== //
    // JSON IMPORT FORMATS //
    // =================== //

    // Time format used in Spotify-distributed JSONs
    public static string SpotifyFull => @"MM/dd/yyyy HH\:mm\:ss"; // 30 day (full acc data) dump
    public static string SpotifyMini => @"yyyy-MM-dd HH\:mm"; // 5 day (past year) dump
    public static DateTimeStyles SpotifyTimeStyle => DateTimeStyles.AssumeUniversal; // Time zone interpretation

    // ================== //
    // GPX EXPORT FORMATS //
    // ================== //

    // Time format used in the <desc> field of GPX song point (your choice)
    public static string DescriptionPlayedAt => @"yyyy-MM-dd HH\:mm\:ss zzz"; // Can be any UTC offset
    public static string DescriptionTimePlayed => @"hh\:mm\:ss\.fff";

    // Time format used in the <time> field of GPX song point (requires ISO 8601):
    public static string GpxOutput => @"yyyy-MM-ddTHH\:mm\:ssZ"; // Must first be converted to UTC
    public static XNamespace OutputNs => "http://www.topografix.com/GPX/1/0"; // Namespace of the output GPX
    public static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance";
    public static string Schema => "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd";

    // =================== //
    // XSPF EXPORT OPTIONS //
    // =================== //

    public static XNamespace Xspf => "http://xspf.org/ns/0/";
}
