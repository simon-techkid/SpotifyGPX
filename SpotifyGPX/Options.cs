// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace SpotifyGPX
{
    /// <summary>
    /// Holds constant values representing user options accessible globally.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// A time in HH:mm:ss format with no date or offset.
        /// </summary>
        public const string TimeOnly = @"HH\:mm\:ss";

        /// <summary>
        /// A date in yyyy-MM-dd and time in HH:mm format with no offset.
        /// </summary>
        public const string DateTimeOnly = @"yyyy-MM-dd HH\:mm";

        /// <summary>
        /// An ISO8601 time in UTC.
        /// DateTimeOffset must be converted to UTC before converting to string.
        /// </summary>
        public const string ISO8601UTC = @"yyyy-MM-ddTHH\:mm\:ssZ";

        /// <summary>
        /// An ISO8601 time including an offset.
        /// Can be any UTC offset.
        /// </summary>
        public const string ISO8601Offset = @"yyyy-MM-dd HH\:mm\:ss zzz";

        /// <summary>
        /// A duration of time in HH:mm:ss.fff format.
        /// </summary>
        public const string TimeSpan = @"hh\:mm\:ss\.fff";

        public static JsonSerializerOptions JsonOptions => new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static JsonSerializerOptions JsonReportOptions => new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IncludeFields = true
        };

        public static XmlWriterSettings XmlSettings => new()
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
            NewLineOnAttributes = false,
            CheckCharacters = true,
            CloseOutput = false,
            WriteEndDocumentOnClose = true,
            Indent = true,
            IndentChars = "\t",
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.None,
            ConformanceLevel = ConformanceLevel.Document,
            NamespaceHandling = NamespaceHandling.Default
        };
    }

    public partial struct LastFmEntry
    {
        private const TimeUsage timeUsage = TimeUsage.Start;
    }

    public partial struct SpotifyEntry
    {
        private const TimeUsage timeUsage = TimeUsage.Start;
    }

    public partial struct XspfEntry
    {
        private const TimeUsage timeUsage = TimeUsage.Start;
    }

    public partial class PairingsHandler
    {
        private static double? MaximumAbsAccuracy => null; // Greatest accepted error (in seconds) between song and point time (null = allow all pairings regardless of accuracy)
    }

    public partial class DupeHandler
    {
        private const int MinimumMatchingCoords = 2; // Minimum number of matching coordinates to be considered a duplicate
    }
}

namespace SpotifyGPX.Input
{
    public partial class InputHandler
    {
        private static ISongInput CreateSongInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".json" => new Json(path),
                ".jsonreport" => new JsonReport(path),
                ".xspf" => new Xspf(path),
                _ => throw new Exception($"Unsupported song file format: {extension}"),
            };
        }

        private static IGpsInput CreateGpsInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".gpx" => new Gpx(path),
                ".kml" => new Kml(path),
                ".jsonreport" => new JsonReport(path),
                _ => throw new Exception($"Unsupported GPS file format: {extension}"),
            };
        }

        private static IPairInput CreatePairInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".jsonreport" => new JsonReport(path),
                _ => throw new Exception($"Unsupported pairs file format: {extension}")
            };
        }
    }

    public partial interface IGpsInput
    {
        private static Dictionary<string, Func<IEnumerable<GpsTrack>, IEnumerable<GpsTrack>>> MultiTrackFilters => new()
        {
            { "A", allTracks => allTracks.Where(track => track.Track.Type == TrackType.GPX) },
            { "B", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Combined) },
            { "C", allTracks => allTracks.Where(track => track.Track.Type == TrackType.Gap) },
            { "D", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Gap) },
            { "E", allTracks => allTracks.Where(track => track.Track.Type != TrackType.GPX) },
            { "F", allTracks => allTracks }
        };

        private static Dictionary<string, string> FilterDefinitions => new()
        {
            { "A", "GPS tracks" },
            { "B", "GPS tracks, and the Gaps between them as tracks" },
            { "C", "Gaps between GPS tracks as tracks" },
            { "D", "GPS tracks and Combined track" },
            { "E", "Gap tracks and Combined track" },
            { "F", "GPS, Gap, and Combined tracks (everything)" }
        };
    }

    public partial class Csv
    {
        private const TimeInterpretation Interpretation = TimeInterpretation.Start;

        // Tolerance filters
        private static readonly Func<LastFmEntry, bool> filter = song => true; // No filtering for CSV files
    }

    public partial class Gpx
    {
        private const LoadOptions loadOptions = LoadOptions.None;
        private static XNamespace InputNs => "http://www.topografix.com/GPX/1/0";
        private const string Track = "trk";
        private const string TrackPoint = "trkpt";
        private const string TimeFormat = $"yyyy-MM-ddTHH:mm:ss.fffzzz";
        private const DateTimeStyles TimeStyle = DateTimeStyles.None;
    }

    public partial class Json
    {
        // Interpretation/parsing options
        private const bool IsMiniSpotify = false; // (true = "Account data", false = "Extended streaming history data")
        private const string SpotifyTimeFormat = IsMiniSpotify ? Options.DateTimeOnly : Options.ISO8601UTC;
        private const DateTimeStyles TimeStyle = DateTimeStyles.AssumeUniversal;
        private const TimeInterpretation Interpretation = TimeInterpretation.End;
        private static JsonSerializerOptions JsonOptions => Options.JsonOptions;

        // Tolerance filters
        private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time (0,0,0 for all songs)
        private static readonly Func<SpotifyEntry, bool> aboveMinPlayTime = song => song.TimePlayed >= MinimumPlaytime; // Song play time should equal or exceed minimum allowed playback threshold

        private const bool ExcludeSkipped = false; // Ignore songs skipped by the user, as defined by Spotify JSON (false for all songs)
        private static readonly Func<SpotifyEntry, bool> notSkipped = song => !ExcludeSkipped || song.Song_Skipped != true; // Song should not be marked as skipped by Spotify

        private readonly Func<SpotifyEntry, bool> filter = song => aboveMinPlayTime(song) && notSkipped(song); // Combined filter function
    }

    public partial class JsonReport
    {
        private static JsonSerializerOptions JsonOptions => Options.JsonReportOptions;

        // Tolerance filters
        private static readonly Func<ISongEntry, bool> songFilter = song => true; // No filtering for JsonReport song-only data
        private static readonly Func<GenericPoint, bool> pointFilter = pair => true; // No filtering for JsonReport song-point data
    }

    public partial class Kml
    {
        private const LoadOptions loadOptions = LoadOptions.None;
        private static XNamespace InputNs => "http://www.opengis.net/kml/2.2";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private const string TimeFormat = $"yyyy-MM-ddTHH:mm:ss.fffZ";
        private const DateTimeStyles TimeStyle = DateTimeStyles.None;
    }

    public partial class Xspf
    {
        private const LoadOptions loadOptions = LoadOptions.None;
        private static XNamespace InputNs => "http://xspf.org/ns/0/";
        private const string Track = "track";
        private const DateTimeStyles TimeStyle = DateTimeStyles.AssumeUniversal;
        private const TimeInterpretation Interpretation = TimeInterpretation.End;

        // Tolerance filters
        private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time (0,0,0 for all songs)
        private static readonly Func<XspfEntry, bool> aboveMinPlayTime = song => song.TimePlayed >= MinimumPlaytime; // Song play time should equal or exceed minimum allowed playback threshold

        private static bool LimitToNonNullOrEmptyURIs => false; // Limit songs to those with non-null, non-empty URIs
        private static readonly Func<XspfEntry, bool> nonNullOrEmptyURI = song => !LimitToNonNullOrEmptyURIs || !string.IsNullOrWhiteSpace(song.Song_URI); // Song should have a non-null, non-empty URI

        private readonly Func<XspfEntry, bool> filter = song => aboveMinPlayTime(song) && nonNullOrEmptyURI(song); // Combined filter function
    }
}

namespace SpotifyGPX.Output
{
    public partial class OutputHandler
    {
        private const bool ReplaceFiles = true;
        private const string AllTracksName = "All";
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 10000;

        private static IFileOutput CreateFileOutput(Formats format, IEnumerable<SongPoint> pairs, string trackName)
        {
            return format switch
            {
                Formats.Csv => new Csv(pairs),
                Formats.Gpx => new Gpx(pairs, trackName),
                Formats.Json => new Json(pairs),
                Formats.JsonReport => new JsonReport(pairs),
                Formats.Kml => new Kml(pairs, trackName),
                Formats.Txt => new Txt(pairs),
                Formats.Xspf => new Xspf(pairs, trackName),
                _ => throw new Exception($"Unsupported file export format: {format}")
            };
        }

        private static bool AllowsMultiTrack(Formats format)
        {
            return format switch
            {
                Formats.Csv => false,
                Formats.Gpx => false,
                Formats.Json => false,
                Formats.JsonReport => true,
                Formats.Kml => false,
                Formats.Txt => false,
                Formats.Xlsx => true,
                Formats.Xspf => false,
                _ => throw new Exception($"Unsupported file export format: {format}")
            };
        }
    }

    public partial class SaveableAndTransformableBase<T>
    {
        private const bool EnableDebugXsltTransformations = false;
        private const bool EnableXsltDocumentFunction = true;
        private const bool EnableXsltScript = true;
    }

    public partial class Csv
    {
        private const string Delimiter = ",";
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Gpx
    {
        private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0";
        private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance";
        private const string Schema = "http://www.topografix.com/GPX/1/0 http://wwwtopografix.com/GPX/1/0/gpx.xsd";
        private const string Waypoint = "wpt";
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Json
    {
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override JsonSerializerOptions JsonOptions => Options.JsonOptions;
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class JsonReport
    {
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override JsonSerializerOptions JsonOptions => Options.JsonReportOptions;
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Kml
    {
        private static XNamespace Namespace => "http://www.opengis.net/kml/2.2";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private const string Placemark = "Placemark";
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Txt
    {
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Xspf
    {
        private static XNamespace Namespace => "http://xspf.org/ns/0/";
        private const string Track = "track";
        private const string Comment = "";
        protected override bool IncludeStylesheetHref => true;
        protected override string StylesheetPath => $"{FormatName}.xslt";
        protected override ReaderOptions XmlReaderOptions => ReaderOptions.None;
        protected override bool ForceUseOfSpecifiedSettings => false;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }
}
