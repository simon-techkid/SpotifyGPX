// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// Handle SongPoint pairings, including list calculation, generation, operations, and exporting.
/// </summary>
public partial class PairingsHandler : BroadcasterBase, IEnumerable<SongPoint>
{
    private List<SongPoint> Pairs { get; set; }

    /// <summary>
    /// The name of this set of pairings.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Create a handler for pairing GPS information with Song information.
    /// </summary>
    public PairingsHandler(string name, Broadcaster bcast) : base(bcast)
    {
        Pairs = new();
        Name = name;
    }

    /// <summary>
    /// Explicitly create a PairingsHandler using an existing pairs list.
    /// </summary>
    /// <param name="pairs">An existing pairs list.</param>
    /// <param name="name">The name of this set of pairs.</param>
    public PairingsHandler(List<SongPoint> pairs, string name, Broadcaster bcast) : base(bcast)
    {
        Pairs = pairs;
        Name = name;
    }

    public void CalculatePairings(List<ISongEntry> s, List<GpsTrack> t, bool predict, bool autoPredict)
    {
        if (predict == true)
        {
            // Let's predict some points!
            DupeHandler dupes = new(PairPoints(s, t), BCaster);
            Pairs = dupes.GetDupes(autoPredict);
        }
        else
        {
            // Nah, just use the verbatim points
            Pairs = PairPoints(s, t);
        }
    }

    public void CalculatePairings(List<SongPoint> pairs)
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
    private List<SongPoint> PairPoints(List<ISongEntry> songs, List<GpsTrack> tracks)
    {
        // Correlate Spotify entries with the nearest GPS points

        int index = 0; // Index of the pairing

        return tracks // For each GPS track
        .SelectMany(track => songs // Get the list of SpotifyEntries
        .Where(songEntry => songEntry.WithinTimeFrame(track.Start, track.End)) // If the song entry falls within the boundaries of the track
        .Select(spotifyEntry => // Select the song entry if it falls in range of the GPS track
            {
                IGpsPoint bestPoint = track.Points
                .OrderBy(point => SongPoint.DisplacementCalculatorAbs(spotifyEntry.Time, point.Time))
                .First();

                SongPoint pair = new(index, spotifyEntry, bestPoint, track.Track);

                BCaster.Broadcast(pair.ToString()); // Notify observers when a pair is created

                index++; // Add to the index of all pairings regardless of track

                return pair;
            })
        )
        .Where(pair => MaximumAbsAccuracy == null || pair.AbsAccuracy <= MaximumAbsAccuracy) // Only create pairings with accuracy equal to or below max allowed accuracy
        .ToList();
    }

    /// <summary>
    /// Write the counts of grouped elements of pairings (ie. no. of pairs by track, type, or country).
    /// </summary>
    public void WriteCounts()
    {
        WriteCounts(pair => pair.Origin, "track", "tracks"); // Write # of pairs per track
        WriteCounts(pair => pair.Origin.Type, "type", "types"); // Write # of pairs in each type of track (Gps, Gap, Combined)
        //WriteCounts(pair => pair.Song.Spotify_Country, "country", "countries"); // Write # of pairs in each country
    }

    private void WriteCounts<T>(Func<SongPoint, T> groupingSelector, string nameSingular, string nameMultiple)
    {
        var groupedPairs = Pairs.GroupBy(groupingSelector); // Group all the song-point pairs by the specified selector
        string countsJoined = string.Join(", ", groupedPairs.Select(group => $"{group.Count()} ({group.Key})"));
        int groupCount = groupedPairs.Count();
        string objName = groupCount == 1 ? nameSingular : nameMultiple;

        BCaster.Broadcast($"Paired {Pairs.Count} songs and points from {groupCount} {objName}: {countsJoined}");
    }

    /// <summary>
    /// Write the averages of grouped elements of pairings (ie. average accuracy by track, track type).
    /// </summary>
    public void WriteAverages()
    {
        WriteAverages(pair => pair.Origin.Type, "track type", "track types"); // Calculate Accuracies by track type (Gps, Gap, Combined)
        WriteAverages(pair => pair.Origin, "track", "tracks"); // Calculate Accuracies by track
    }

    private void WriteAverages<T>(Func<SongPoint, T> groupingSelector, string nameSingular, string nameMultiple)
    {
        var groupedPairs = Pairs.GroupBy(groupingSelector); // Group all the song-point pairs by the specified selector
        string accuraciesJoined = string.Join(", ", groupedPairs.Select(group => $"{Math.Round(group.Average(pair => pair.AbsAccuracy))}s ({group.Key})"));
        int groupCount = groupedPairs.Count();
        string objName = groupCount == 1 ? nameSingular : nameMultiple;

        BCaster.Broadcast($"Average Accuracy for {groupCount} {objName}: {accuraciesJoined}");
    }

    /// <summary>
    /// Check all the pairings for Easter eggs.
    /// </summary>
    public void CheckEasterEggs()
    {
        WriteEggs(new SongEasterEggs(BCaster));
    }

    private void WriteEggs<T>(EasterEggs<T> egg)
    {
        egg.CheckAllPairsForEggs(Pairs);
    }

    public IEnumerator<SongPoint> GetEnumerator()
    {
        return Pairs.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
