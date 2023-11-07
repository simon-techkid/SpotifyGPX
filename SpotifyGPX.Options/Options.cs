// SpotifyGPX by Simon Field

using System;

#nullable enable

namespace SpotifyGPX.Options;

public class Point
{
    // Time format as read from the input GPX file:
    public static readonly string gpxPointTimeInp = "yyyy-MM-ddTHH:mm:ss.fffzzz"; // time format used to interpret GPX track <time> tags

    // Time format of the Spotify track portrayed in the GPX point description:
    public static readonly string gpxPointDescription = "yyyy-MM-dd HH:mm:ss zzz"; // time format used in the <desc> field a GPX song point (your choice)

    // Time format used when writing to the GPX in fields that require ISO 8601:
    public static readonly string gpxTimeOut = "yyyy-MM-ddTHH:mm:ss.fffZ";

    // Time format for console readout of point-song time comparison:
    public static readonly string consoleReadoutFormat = "HH:mm:ss";

    public static string GpxTitle(SpotifyEntry song)
    {
        // ============== \\
        // GPX POINT NAME \\
        // ============== \\

        return $"{song.Song_Artist} - {song.Song_Name}";
    }

    public static string GpxDescription(SongPoint pair)
    {
        // Function defining the preferred return strings of GPX point metadata

        // Spotify Backend
        string? Username = pair.Song.Spotify_Username; // This field is your Spotify username.
        string? Platform = pair.Song.Spotify_Platform; // This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast).
        string? Origin_Country = pair.Song.Spotify_Country; // This field is the country code of the country where the stream was played (e.g. SE - Sweden).
        string? IP_Address = pair.Song.Spotify_IP; // This field contains the IP address logged when streaming the track.
        string? User_Agent = pair.Song.Spotify_UA; // This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari)
        bool? Offline = pair.Song.Spotify_Offline; // This field indicates whether the track was played in offline mode (“True”) or not (“False”).
        string? Offline_Timestamp = pair.Song.Spotify_OfflineTS; // This field is a timestamp of when offline mode was used, if used.
        bool? Incognito = pair.Song.Spotify_Incognito; // This field indicates whether the track was played in incognito mode (“True”) or not (“False”).

        // Track Metadata
        string? Title = pair.Song.Song_Name; // This field is the name of the track.
        string? Artist = pair.Song.Song_Artist; // This field is the name of the artist, band or podcast.
        string? Album = pair.Song.Song_Album; // This field is the name of the album of the track.
        string? URL = pair.Song.Song_URI; // A Spotify URI, uniquely identifying the track in the form of “spotify:track:<base-62 string>”
        string? StartReason = pair.Song.Song_StartReason; // This field is a value telling why the track started (e.g. “trackdone”)
        string? EndReason = pair.Song.Song_EndReason; // This field is a value telling why the track ended (e.g. “endplay”).
        bool? Shuffled = pair.Song.Song_Shuffle; // This field has the value True or False depending on if shuffle mode was used when playing the track.
        bool? Skipped = pair.Song.Song_Skipped; // This field indicates if the user skipped to the next song

        // Episode Metadata
        string? Episode_Title = pair.Song.Episode_Name; // This field contains the name of the episode of the podcast.
        string? Episode_Show = pair.Song.Episode_Show; // This field contains the name of the show of the podcast.
        string? Episode_URL = pair.Song.Episode_URI; // A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:<base-62 string>”

        // Duration Information
        DateTimeOffset EndedAt = new(pair.Song.Time_End.Ticks + pair.Point.Time.Offset.Ticks, pair.Point.Time.Offset); // This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time
        string? PlayedMs = pair.Song.Time_Played; // This field is the number of milliseconds the stream was played.

        string returnString = "";

        // ===================== \\
        // GPX POINT DESCRIPTION \\
        // ===================== \\

        string description = "";

        description += $"{(pair.Song.Time_End != null ? $"Ended here, at {EndedAt.ToString(gpxPointDescription)}" : null)}";
        description += $"\nSong is {Math.Abs(pair.Accuracy)} seconds {(pair.Accuracy < 0 ? "behind the" : "ahead of the")} point";
        description += $"{(pair.Song.Song_Shuffle != null ? $"\nShuffle: {(Shuffled == true ? "On" : "Off")}" : null)}";
        description += $"{(pair.Song.Song_Skipped != null ? $"\nSkipped: {(Skipped == true ? "Yes" : "No")}" : null)}";
        description += $"{(pair.Song.Spotify_Offline != null ? $"\nOffline: {(Offline == true ? "Yes" : "No")}" : null)}";
        description += $"{(pair.Song.Spotify_IP != null ? $"\nIP Address: {IP_Address}" : null)}";
        description += $"{(pair.Song.Spotify_Country != null ? $"\nCountry: {Origin_Country}" : null)}";
        description += $"{(pair.Point.Predicted == true ? $"\nPoint Predicted" : null)}";

        returnString += description;

        return returnString;
    }
}

public class Playlist
{
    // Time format used for the XSPF time ended
    public static readonly string xspfTime = Point.gpxPointTimeInp;

    public static string? Tag(SpotifyEntry song, int tag)
    {
        string? Creator = song.Song_Artist;
        string? Title = song.Song_Name;
        string? Annotation = song.Time_End.ToString(xspfTime);
        string? Duration = song.Time_Played;

        if (tag == 1) { return Creator; }
        if (tag == 2) { return Title; }
        if (tag == 3) { return Annotation; }
        if (tag == 4) { return Duration; }

        // No applicable return string
        throw new Exception("Could not retrieve tag format from options");
    }
}