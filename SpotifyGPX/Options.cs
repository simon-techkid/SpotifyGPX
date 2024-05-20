// SpotifyGPX by Simon Field

using OfficeOpenXml.Table;
using SpotifyGPX.Input;
using SpotifyGPX.Output;
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

    public partial class Program
    {
        private static Dictionary<Formats, bool> GetExportOptions(HashSet<string>? flags)
        {
            return new Dictionary<Formats, bool>
            {
                { Formats.Csv, flags?.Contains("c") ?? false }, // Set any value to true to export by default.
                { Formats.Gpx, flags?.Contains("g") ?? false },
                { Formats.Json, flags?.Contains("j") ?? false },
                { Formats.JsonReport, flags?.Contains("r") ?? false },
                { Formats.Kml, flags?.Contains("k") ?? false },
                { Formats.Tsv, flags?.Contains("v") ?? false },
                { Formats.Txt, flags?.Contains("t") ?? false },
                { Formats.Xlsx, flags?.Contains("e") ?? false },
                { Formats.Xspf, flags?.Contains("p") ?? false }
            };
        }
    }

    public partial class ArgumentParser
    {
        private static string Help
        {
            get
            {
                System.Text.StringBuilder builder = new();
                builder.AppendLine("Usage: SpotifyGPX [--spotify <spotify> --gps <gps>] [--pairs <pairs>] [-c] [-g] [-j] [-k] [-p] [-v] [-t] [-r] [-e] [-x] [-pp [-pa]] [-s] [-h]");
                builder.AppendLine("--spotify <spotify> --gps <gps> - Path to a Spotify playback history and GPS journey file");
                builder.AppendLine("--pairs <pairs> - Path to a pairs file");
                builder.AppendLine("-c - Export a CSV table of all the pairs");
                builder.AppendLine("-g - Export a GPX from the calculated points");
                builder.AppendLine("-j - Save off the relevant part of the Spotify json");
                builder.AppendLine("-k - Export a KML from the calculated points");
                builder.AppendLine("-p - Export a XSPF playlist of the songs");
                builder.AppendLine("-v - Export a TSV table of all the pairs");
                builder.AppendLine("-t - Export a plain text list of pairs");
                builder.AppendLine("-r - Export a JsonReport of all the data used to compile the resulting pairings");
                builder.AppendLine("-e - Export an Excel workbook of all pairings, grouped into worksheets for each track");
                builder.AppendLine("-x - Export an XML conversion of each file exported (combine this with other format export flags)");
                builder.AppendLine("-pp - Predict new positions for duplicate points (use with -pa for automatic prediction of all duplicate positions)");
                builder.AppendLine("-s - Do not print out each newly created Song-Point pairing upon creation");
                builder.AppendLine("-h - Print the help instructions");
                return builder.ToString();
            }
        }
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
                ".csv" => new Csv(path),
                ".json" => new Json(path),
                ".jsonreport" => new JsonReport(path),
                ".random" => new SongTest(),
                ".xspf" => new Xspf(path),
                _ => throw new Exception($"Unsupported song file format: {extension}"),
            };
        }

        private static IGpsInput CreateGpsInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".geojson" => new GeoJson(path),
                ".gpx" => new Gpx(path),
                ".kml" => new Kml(path),
                ".jsonreport" => new JsonReport(path),
                ".random" => new PointTest(),
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

    public partial class SongInputBase
    {
        private const TimeInterpretation DefaultInterpretation = TimeInterpretation.Start;
    }

    public partial interface IGpsInput
    {
        private static Dictionary<string, Func<IEnumerable<GpsTrack>, IEnumerable<GpsTrack>>> MultiTrackFilters => new()
        {
            { "A", allTracks => allTracks.Where(track => track.Track.Type == TrackType.Gps) },
            { "B", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Combined) },
            { "C", allTracks => allTracks.Where(track => track.Track.Type == TrackType.Gap) },
            { "D", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Gap) },
            { "E", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Gps) },
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
        protected override TimeInterpretation Interpretation => TimeInterpretation.Start;

        // Tolerance filters
        private static readonly Func<LastFmEntry, bool> filter = song => true; // No filtering for CSV files
    }

    public partial class GeoJson
    {
        private static readonly Func<GeoJsonPoint, bool> filter = point => true; // No filtering
    }

    public partial class Gpx
    {
        private const LoadOptions loadOptions = LoadOptions.None;
        private static XNamespace InputNs => "http://www.topografix.com/GPX/1/0";
        private const string Track = "trk";
        private const string TrackPoint = "trkpt";
        private const string TimeFormat = $"yyyy-MM-ddTHH:mm:ss.fffzzz";
        private const DateTimeStyles TimeStyle = DateTimeStyles.None;

        // Tolerance filters
        private static readonly Func<GpxPoint, bool> filter = point => true; // No filtering
    }

    public partial class Json
    {
        // Interpretation/parsing options
        private const bool IsMiniSpotify = false; // (true = "Account data", false = "Extended streaming history data")
        private const string SpotifyTimeFormat = IsMiniSpotify ? Options.DateTimeOnly : Options.ISO8601UTC;
        private const DateTimeStyles TimeStyle = DateTimeStyles.AssumeUniversal;
        protected override TimeInterpretation Interpretation => TimeInterpretation.End;
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
        private static readonly Func<SongPoint, bool> pairFilter = pair => true; // No filtering for JsonReport song-point data
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

        // Tolerance filters
        private static readonly Func<KmlPoint, bool> filter = point => true; // No filtering
    }

    public partial class PointTest
    {
        // More options for PointTest available in the PointTest.cs file.

        // Number of days of points prior to today to generate
        private const double DaysPriorToTodayToGenerate = 3;

        // Center point generation location - Libourne, France
        private const double CenterLat = 44.918516;
        private const double CenterLon = -0.245090;
        private const int CenterRadius = 20; // Radius from center in kilometers
        
        // Point placement intervals
        private const int MinPlacementSecs = 15; // Minimum GPS point placement interval in seconds
        private const int MaxPlacementSecs = 120; // Maximum GPS point placement interval in seconds

        // Drive start (randomness floor) time (currently 08:00) - depart at 08:00 at the earliest
        private const int DriveMinStartHour = 8;
        private const int DriveMinStartMinute = 0;

        // Drive start (randomness ceiling) time (currently 10:30) - depart at 10:30 at the latest
        private const int DriveMaxStartHour = 10;
        private const int DriveMaxStartMinute = 30;

        // This means that the drive will start anywhere between 08:00 and 10:30

        // Drive end (randomness floor) time (currently 17:00) - arrive at 17:00 at the earliest
        private const int DriveMinEndHour = 17;
        private const int DriveMinEndMinute = 0;

        // Drive end (randomness ceiling) time (currently 19:00) - arrive at 19:00 at the latest
        private const int DriveMaxEndHour = 19;
        private const int DriveMaxEndMinute = 0;

        // This means that the drive will end anywhere between 17:00 and 19:00
    }

    public partial class SongTest
    {
        // More options for SongTest available in the SongTest.cs file.

        // Number of days opf songs prior to today to generate
        private const double DaysPriorToTodayToGenerate = 365;

        // Number of unique artists and songs to generate
        private const int ArtistsCount = 10;
        private const int SongsCount = 100;
        
        // Song playback start (randomness floor) time (currently 07:00) - start playing at 07:00 at the earliest
        private const int PlaybackMinStartHour = 5;
        private const int PlaybackMinStartMinute = 0;

        // Song playback start (randomness ceiling) time (currently 07:30) - start playing at 07:30 at the latest
        private const int PlaybackMaxStartHour = 7;
        private const int PlaybackMaxStartMinute = 30;

        // Song playback end (randomness floor) time (currently 20:00) - stop playing at 20:00 at the earliest
        private const int PlaybackMinEndHour = 20;
        private const int PlaybackMinEndMinute = 0;

        // Song playback end (randomness ceiling) time (currently 23:00) - stop playing at 23:00 at the latest
        private const int PlaybackMaxEndHour = 23;
        private const int PlaybackMaxEndMinute = 0;
    }

    public partial class Xspf
    {
        private const LoadOptions loadOptions = LoadOptions.None;
        private static XNamespace InputNs => "http://xspf.org/ns/0/";
        private const string Track = "track";
        private const DateTimeStyles TimeStyle = DateTimeStyles.AssumeUniversal;
        protected override TimeInterpretation Interpretation => TimeInterpretation.End;

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

        private static bool AllowsMultiTrack(Formats format)
        {
            return format switch
            {
                Formats.Csv => false,
                Formats.Gpx => false,
                Formats.Json => false,
                Formats.JsonReport => true,
                Formats.Kml => false,
                Formats.Tsv => false,
                Formats.Txt => false,
                Formats.Xlsx => true,
                Formats.Xspf => false,
                _ => throw new Exception($"Unsupported file export format: {format}")
            };
        }
    }

    public partial class FileOutputFactory
    {
        private readonly Dictionary<Formats, FileOutputCreator> creators = new()
        {
            { Formats.Csv, (pairs, trackName) => new Csv(pairs, trackName) },
            { Formats.Gpx, (pairs, trackName) => new Gpx(pairs, trackName) },
            { Formats.Json, (pairs, trackName) => new Json(pairs, trackName) },
            { Formats.JsonReport, (pairs, trackName) => new JsonReport(pairs, trackName) },
            { Formats.Kml, (pairs, trackName) => new Kml(pairs, trackName) },
            { Formats.Tsv, (pairs, trackName) => new Tsv(pairs, trackName) },
            { Formats.Txt, (pairs, trackName) => new Txt(pairs, trackName) },
            { Formats.Xlsx, (pairs, trackName) => new Xlsx(pairs, trackName) },
            { Formats.Xspf, (pairs, trackName) => new Xspf(pairs, trackName) }
        };
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

        private readonly List<ColumnDefinition> columns = new()
        {
                new() { Heading = "Title", CellValue = pair => pair.Song.Song_Name },
                new() { Heading = "Artist", CellValue = pair => pair.Song.Song_Artist },
                new() { Heading = "Latitude", CellValue = pair => pair.Point.Location.Latitude },
                new() { Heading = "Longitude", CellValue = pair => pair.Point.Location.Longitude },
                new() { Heading = "Song Time", CellValue = pair => pair.SongTime.ToString() },
                new() { Heading = "Point Time", CellValue = pair => pair.PointTime.ToString() }
        };

        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Gpx
    {
        private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0";
        private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance";
        private const string Schema = "http://www.topografix.com/GPX/1/0 http://wwwtopografix.com/GPX/1/0/gpx.xsd";
        private const string Waypoint = "wpt";
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Json
    {
        protected override JsonSerializerOptions JsonOptions => Options.JsonOptions;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class JsonReport
    {
        protected override JsonSerializerOptions JsonOptions => Options.JsonReportOptions;
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Kml
    {
        private static XNamespace Namespace => "http://www.opengis.net/kml/2.2";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private const string Placemark = "Placemark";
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Tsv
    {
        private const string Delimiter = "\t";

        private readonly List<ColumnDefinition> columns = new()
        {
                new() { Heading = "Title", CellValue = pair => pair.Song.Song_Name },
                new() { Heading = "Artist", CellValue = pair => pair.Song.Song_Artist },
                new() { Heading = "Latitude", CellValue = pair => pair.Point.Location.Latitude },
                new() { Heading = "Longitude", CellValue = pair => pair.Point.Location.Longitude },
                new() { Heading = "Song Time", CellValue = pair => pair.SongTime.ToString() },
                new() { Heading = "Point Time", CellValue = pair => pair.PointTime.ToString() }
        };

        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Txt
    {
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }

    public partial class Xlsx
    {
        // TotalFormula Formatting:
        // {0} = Column Letter
        // {1} = Name of the column header
        // {2} = Number of the first row in this column
        // {3} = Number of the last row in this column

        // Default table placement: top right corner of the table in cell A1, StartColumn = 0 and StartRow = 1

        // Worksheet Settings
        private const bool ColorSheets = true; // Add random colors to each track worksheet tab

        // Table Settings
        private const TableStyles TableStyle = TableStyles.Light1;
        private const int StartColumn = 0; // The first column of each sheet on which to place the table
        private const int StartRow = 1; // The first row of each sheet on which to place the table
        private const bool CreateTotals = true; // Add total row to each track table

        // TotalFormula
        // Below commented are only compatible with Excel. They select the column by name.
        private const string StringMode = "INDEX({0}{2}:{0}{3}, MODE.SNGL(MATCH({0}{2}:{0}{3}, {0}{2}:{0}{3}, 0)))";
        private const string LastToFirstDiff = "INDEX({0}{2}:{0}{3}, COUNTA({0}{2}:{0}{3})) - INDEX({0}{2}:{0}{3}, 1)";
        private const string Sum = "SUBTOTAL(109,{0}{2}:{0}{3})";
        private const string Avg = "SUBTOTAL(101,{0}{2}:{0}{3})";
        //private const string StringMode = "INDEX([{1}], MODE.SNGL(MATCH([{1}], [{1}], 0)))";
        //private const string LastToFirstDiff = "INDEX([{1}], COUNTA([{1}])) - INDEX([{1}], 1)";
        //private const string Sum = "SUBTOTAL(109,[{1}])";
        //private const string Avg = "SUBTOTAL(101,[{1}])";

        private const bool ConditionalFormatting = true;

        // NumberFormat
        private const string TimeSpan = "[h]:mm:ss";
        private const string TimeFormat = "yyyy-mm-dd hh:mm:ss";
        private const string Double = "0.00";
    }

    public partial class Xspf
    {
        private static XNamespace Namespace => "http://xspf.org/ns/0/";
        private const string Track = "track";
        private const string Comment = "";
        protected override XmlWriterSettings XmlSettings => Options.XmlSettings;
    }
}
