// SpotifyGPX by Simon Field

using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// Handle SongPoint pairings, including list calculation, generation, operations, and exporting.
/// </summary>
public partial class PairingsHandler
{
    private List<SongPoint> Pairs { get; }

    /// <summary>
    /// Create a handler for pairing GPS information with Song information.
    /// </summary>
    /// <param name="s">A series of Spotify playback records, used to associate a song with a point.</param>
    /// <param name="t">A list of GPXTrack objects, used to associate songs with a track and position on Earth.</param>
    /// <param name="silent">If true, do not print each pairing to the console upon creation.</param>
    /// <param name="predict">If true, create a DupeHandler for predicting duplicates in the resulting pair list.</param>
    /// <param name="autoPredict">If true, and predict is true, automatically predict all duplicate positions.</param>
    public PairingsHandler(List<ISongEntry> s, List<GPXTrack> t, bool silent, bool predict, bool autoPredict)
    {
        if (predict == true)
        {
            // Let's predict some points!
            DupeHandler dupes = new(PairPoints(silent, s, t));
            Pairs = dupes.GetDupes(autoPredict);
        }
        else
        {
            // Nah, just use the verbatim points
            Pairs = PairPoints(silent, s, t);
        }
    }

    /// <summary>
    /// Explicitly create a PairingsHandler using an existing pairs list.
    /// </summary>
    /// <param name="pairs">An existing pairs list.</param>
    public PairingsHandler(List<SongPoint> pairs)
    {
        Pairs = pairs;
    }

    /// <summary>
    /// Pair songs with points (positions on Earth), by finding the closest gap of time between each.
    /// </summary>
    /// <param name="silent">If true, do not print each pairing to the console upon creation.</param>
    /// <param name="songs">A series of Spotify playback records, used to associate a song with a point.</param>
    /// <param name="tracks">A list of GPXTrack objects, used to associate songs with a track and position on Earth.</param>
    /// <returns>A list of Song-Point pairs, each song and its point (on Earth), in a list.</returns>
    private static List<SongPoint> PairPoints(bool silent, List<ISongEntry> songs, List<GPXTrack> tracks)
    {
        // Correlate Spotify entries with the nearest GPX points

        int index = 0; // Index of the pairing

        return tracks // For each GPX track
        .SelectMany(track => songs // Get the list of SpotifyEntries
        .Where(spotifyEntry => spotifyEntry.WithinTimeFrame(track.Start, track.End)) // If the SpotifyEntry falls within the boundaries of the track
        .Select(spotifyEntry => // Select the Spotify entry if it falls in range of the GPX track
            {
                SongPoint pair = track.Points
                .Select(point => new SongPoint(index, spotifyEntry, point, track.Track)) // For each point in the track's point list,
                .OrderBy(pair => pair.AbsAccuracy) // Order the points by proximity between point and song
                .First(); // Closest accuracy wins

                if (!silent)
                {
                    Console.WriteLine(pair.ToString());
                }

                index++; // Add to the index of all pairings regardless of track

                return pair;
            })
        )
        .Where(pair => MaximumAbsAccuracy == null || pair.AbsAccuracy <= MaximumAbsAccuracy) // Only create pairings with accuracy equal to or below max allowed accuracy
        .ToList();
    }

    /// <summary>
    /// Save the paired songs and points to a file.
    /// </summary>
    /// <param name="format">The file format in which the pairs will be saved.</param>
    /// <param name="sourceGpxName">The name of the original GPX file.</param>
    public void Save(Formats format, string sourceGpxName, bool transform)
    {
        // Parse the Pairs in this Pairings object to the specified format, according to user-provided arguments
        OutputHandler fmat = new(Pairs);

        // Save the handled format as specified by the provided file name prefix
        fmat.Save(format, sourceGpxName, transform);
    }

    /// <summary>
    /// Write the counts of grouped elements of pairings (ie. no. of pairs by track, type, or country).
    /// </summary>
    public void WriteCounts()
    {
        WriteCounts(pair => pair.Origin, "track", "tracks"); // Write # of pairs per track
        WriteCounts(pair => pair.Origin.Type, "type", "types"); // Write # of pairs in each type of track (GPX, Gap, Combined)
        //WriteCounts(pair => pair.Song.Spotify_Country, "country", "countries"); // Write # of pairs in each country
    }

    /// <summary>
    /// Write the number of pairs in each group (based on a selector) to the console.
    /// </summary>
    /// <typeparam name="T">The object of a pair by which all pairs should be grouped.</typeparam>
    /// <param name="groupingSelector">The grouping selector, the parameter of each pair by which the collection of pairs should be grouped.</param>
    /// <param name="nameSingular">The name of one of these groups.</param>
    /// <param name="nameMultiple">The name of multiple of these groups.</param>
    private void WriteCounts<T>(Func<SongPoint, T> groupingSelector, string nameSingular, string nameMultiple)
    {
        var groupedPairs = Pairs.GroupBy(groupingSelector); // Group all the song-point pairs by the specified selector
        string countsJoined = string.Join(", ", groupedPairs.Select(group => $"{group.Count()} ({group.Key})"));
        int groupCount = groupedPairs.Count();
        string objName = groupCount == 1 ? nameSingular : nameMultiple;

        Console.WriteLine($"[PAIR] Paired {Pairs.Count} songs and points from {groupCount} {objName}: {countsJoined}");
    }

    /// <summary>
    /// Write the averages of grouped elements of pairings (ie. average accuracy by track, track type).
    /// </summary>
    public void WriteAverages()
    {
        WriteAverages(pair => pair.Origin.Type, "track type", "track types"); // Calculate Accuracies by track type (GPX, Gap, Combined)
        WriteAverages(pair => pair.Origin, "track", "tracks"); // Calculate Accuracies by track
    }

    /// <summary>
    /// Write the average accuracies (in seconds, between each song and point in a pair) to the console in groups (based on a selector).
    /// </summary>
    /// <typeparam name="T">The object of a pair by which all pairs should be grouped.</typeparam>
    /// <param name="groupingSelector">The grouping selector, the parameter of each pair by which the collection of pairs should be grouped.</param>
    /// <param name="nameSingular">The name of one of these groups.</param>
    /// <param name="nameMultiple">The name of multiple of these groups.</param>
    private void WriteAverages<T>(Func<SongPoint, T> groupingSelector, string nameSingular, string nameMultiple)
    {
        var groupedPairs = Pairs.GroupBy(groupingSelector); // Group all the song-point pairs by the specified selector
        string accuraciesJoined = string.Join(", ", groupedPairs.Select(group => $"{Math.Round(group.Average(pair => pair.AbsAccuracy))}s ({group.Key})"));
        int groupCount = groupedPairs.Count();
        string objName = groupCount == 1 ? nameSingular : nameMultiple;

        Console.WriteLine($"[PAIR] Average Accuracy for {groupCount} {objName}: {accuraciesJoined}");
    }
}