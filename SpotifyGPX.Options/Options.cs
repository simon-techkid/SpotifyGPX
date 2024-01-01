// SpotifyGPX by Simon Field

#nullable enable

namespace SpotifyGPX.Options;

public class Point
{
    // Time format for console printing of point-song time comparison:
    public static readonly string consoleReadoutFormat = "HH:mm:ss";


    // ========================== //
    // GPX JOURNEY IMPORT FORMATS //
    // ========================== //

    // Time format used to interpret your GPX track <time> tags
    public static readonly string gpxTimeInp = "yyyy-MM-ddTHH:mm:ss.fffzzz";


    // =========================== //
    // SPOTIFY JSON IMPORT FORMATS //
    // =========================== //

    // Time format used in Spotify-distributed JSONs
    public static readonly string fullSpotFormat = "MM/dd/yyyy HH:mm:ss"; // 30 day (full acc data) dump
    public static readonly string miniSpotFormat = "yyyy-MM-dd HH:mm"; // 5 day (past year) dump


    // =========================== //
    // GPX PAIRINGS EXPORT FORMATS //
    // =========================== //

    // Time format used in the <desc> field of GPX song point (your choice)
    public static readonly string gpxPointDescription = "yyyy-MM-dd HH:mm:ss zzz";

    // Time format used in the <time> field of GPX song point (requires ISO 8601):
    public static readonly string gpxTimeOut = "yyyy-MM-ddTHH:mm:ss.fffZ";
}
