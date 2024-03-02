// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System.Globalization;
using System.Xml.Linq;
using System.Text;

namespace SpotifyGPX
{
    /// <summary>
    /// Holds constant values representing user options accessible globally.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Interpret this parsed string as a universal (UTC) time.
        /// </summary>
        public static DateTimeStyles InterpretAsUniversal => DateTimeStyles.AssumeUniversal;

        /// <summary>
        /// Interpret this parsed string as a local (system) time.
        /// </summary>
        public static DateTimeStyles InterpretAsLocal => DateTimeStyles.AssumeLocal;

        /// <summary>
        /// Interpret this parsed string as a time in any timezone, converted to UTC.
        /// </summary>
        public static DateTimeStyles ConvertToUniversal => DateTimeStyles.AdjustToUniversal;

        // Time Outputs

        /// <summary>
        /// A time in HH:mm:ss format with no date or offset.
        /// </summary>
        public static string TimeOnly => @"HH\:mm\:ss";

        /// <summary>
        /// A date in yyyy-MM-dd and time in HH:mm format with no offset.
        /// </summary>
        public static string DateTimeOnly => @"yyyy-MM-dd HH\:mm";

        /// <summary>
        /// An ISO8601 time in UTC.
        /// DateTimeOffset must be converted to UTC before converting to string.
        /// </summary>
        public static string ISO8601UTC => @"yyyy-MM-ddTHH\:mm\:ssZ";

        /// <summary>
        /// An ISO8601 time including an offset
        /// Can be any UTC offset.
        /// </summary>
        public static string ISO8601Offset => @"yyyy-MM-dd HH\:mm\:ss zzz";

        /// <summary>
        /// A duration of time in HH:mm:ss.fff format.
        /// </summary>
        public static string TimeSpan => @"hh\:mm\:ss\.fff";
    }

    public readonly partial struct SpotifyEntry
    {
        private static bool PreferEstimatedStartTime => true; // Estimate the start time of the track by subtracting the time played duration from the end time (assumes the user didn't pause playback)
    }

    public partial class PairingsHandler
    {
        private static double? MaximumAbsAccuracy => null; // Greatest accepted error (in seconds) between song and point time (null = allow all pairings regardless of accuracy)
    }

    public partial class DupeHandler
    {
        private static int MinimumMatchingCoords => 2; // Minimum number of matching coordinates to be considered a duplicate
    }
}


namespace SpotifyGPX.Input
{
    public partial class Gpx
    {
        private static XNamespace InputNs => "http://www.topografix.com/GPX/1/0"; // Namespace of input GPX
        private static string Track => "trk"; // Name of a track element
        private static string TrackPoint => "trkpt"; // Name of a track point object, children of tracks
        private static string TimeFormat => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz";
        private static DateTimeStyles TimeStyle => DateTimeStyles.None;
    }

    public partial class Json
    {
        private static JsonSerializerSettings JsonSettings => new()
        {
            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            NullValueHandling = NullValueHandling.Include
        };
    }

    public partial class JsonReport
    {
        private static JsonSerializerSettings JsonSettings => new()
        {
            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            NullValueHandling = NullValueHandling.Include
        };
    }
}

namespace SpotifyGPX.Output
{
    public partial class OutputHandler
    {
        private static bool ReplaceFiles => true; // Allow SpotifyGPX to replace existing files, rather than generating a unique name
        private static string AllTracksName => "All"; // Prefix of a file containing multiple tracks
    }

    public partial class Csv
    {
        private static string Delimiter => ","; // Delimiter of the output CSV
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output CSV
    }
    
    public partial class Gpx
    {
        private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0"; // Namespace of the output GPX
        private static string DocumentEncoding => "utf-8";
        private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance"; // XML schema location of the output GPX
        private static string Schema => "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd"; // GPX schema location(s) of the output GPX
        private static string Waypoint => "wpt"; // Name of a waypoint object
        private static SaveOptions OutputSettings => SaveOptions.None;
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output XSPF
    }

    public partial class Html
    {
        private static XNamespace Namespace => "http://www.w3.org/1999/xhtml";
        private static string DocumentEncoding => "utf-8";
        private static string CSS => @"
body {
  background-color: #E4F3FF;
  font-family: sans-serif, Helvetica, Arial;
  font-size: 13px;
}
h1 {
  color: #2D58AE;
  font-size: 25px;
}
hr {
  color: #555555;
}
";
        private static SaveOptions OutputSettings => SaveOptions.None;
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output XSPF
    }

    public partial class Json
    {
        private static Formatting OutputFormatting => Formatting.Indented;
        private static JsonSerializerSettings JsonSettings => new()
        {
            
        };
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output JSON
    }

    public partial class JsonReport
    {
        private static Formatting OutputFormatting => Formatting.Indented;
        private static JsonSerializerSettings JsonSettings => new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.ffffffK",
            NullValueHandling = NullValueHandling.Include,
        };
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output JSON
    }

    public partial class Txt
    {
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output TXT
    }

    public partial class Xspf
    {
        private static XNamespace Namespace => "http://xspf.org/ns/0/"; // Namespace of output XSPF
        private static string DocumentEncoding => "utf-8";
        private static string Track => "track"; // Name of a track object
        private static SaveOptions OutputSettings => SaveOptions.None;
        private static Encoding OutputEncoding => Encoding.UTF8; // Encoding of the output XSPF
    }
}