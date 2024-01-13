// SpotifyGPX by Simon Field

namespace SpotifyGPX;

public struct Formats
{
    // Time format for console printing of point-song time comparison:
    public static readonly string consoleReadoutFormat = @"HH\:mm\:ss";

    // ================== //
    // GPX IMPORT FORMATS //
    // ================== //

    // Time format used to interpret your GPX track <time> tags
    public static readonly string gpxTimeInp = @"yyyy-MM-ddTHH\:mm\:ss.fffzzz";

    // =================== //
    // JSON IMPORT FORMATS //
    // =================== //

    // Time format used in Spotify-distributed JSONs
    public static readonly string fullSpotFormat = @"MM/dd/yyyy HH\:mm\:ss"; // 30 day (full acc data) dump
    public static readonly string miniSpotFormat = @"yyyy-MM-dd HH\:mm"; // 5 day (past year) dump

    // =================== //
    // JSON EXPORT FORMATS //
    // =================== //
    public static readonly string outJsonFormat = @"yyyy-MM-ddTHH\:mm\:ssZ";

    // ================== //
    // GPX EXPORT FORMATS //
    // ================== //

    // Time format used in the <desc> field of GPX song point (your choice)
    public static readonly string gpxDescriptionPlayedAt = @"yyyy-MM-dd HH\:mm\:ss zzz";
    public static readonly string gpxDescriptionTimePlayed = @"hh\:mm\:ss\.fff";

    // Time format used in the <time> field of GPX song point (requires ISO 8601):
    public static readonly string gpxTimeOut = @"yyyy-MM-ddTHH\:mm\:ssZ";
}
