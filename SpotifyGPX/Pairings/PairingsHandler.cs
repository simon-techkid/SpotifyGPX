// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyGPX.Pairings;

/// <summary>
/// Handle SongPoint pairings, including list calculation, generation, operations, and exporting.
/// </summary>
public abstract partial class PairingsHandler : StringBroadcasterBase, IEnumerable<SongPoint>
{
    protected List<SongPoint> Pairs { get; set; }

    /// <summary>
    /// The name of this set of pairings.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Create a handler for pairing GPS information with Song information.
    /// </summary>
    protected PairingsHandler(StringBroadcaster bcast) : base(bcast)
    {
        Pairs = new();
    }

    /// <summary>
    /// Calculate a set (list) of <see cref="SongPoint"/> objects given a list of <see cref="ISongEntry"/> and <see cref="GpsTrack"/> objects.
    /// </summary>
    /// <param name="s">A list of songs in the form of <see cref="ISongEntry"/> objects.</param>
    /// <param name="t">A list of GPS tracks in the form of <see cref="GpsTrack"/> objects.</param>
    public virtual void CalculatePairings(List<ISongEntry> s, List<GpsTrack> t) => Pairs = PairPoints(s, t);

    /// <summary>
    /// Calculate a set (list) of <see cref="SongPoint"/> objects given an existing list of <see cref="SongPoint"/> objects.
    /// </summary>
    /// <param name="pairs">A list of pairs in the form of <see cref="SongPoint"/> objects.</param>
    public virtual void CalculatePairings(List<SongPoint> pairs) => Pairs = pairs;

    /// <summary>
    /// Pair songs with points (positions on Earth), by finding the closest gap of time between each.
    /// </summary>
    /// <param name="songs">A series of Spotify playback records, used to associate a song with a point.</param>
    /// <param name="tracks">A list of GPXTrack objects, used to associate songs with a track and position on Earth.</param>
    /// <returns>A list of Song-Point pairs, each song and its point (on Earth), in a list.</returns>
    protected List<SongPoint> PairPoints(List<ISongEntry> songs, List<GpsTrack> tracks)
    {
        int index = 0; // Index of the pairing
        List<SongPoint> pairs = new();

        // Preprocess tracks to sort points by time
        List<GpsTrack> preprocessedTracks = tracks.Select(track =>
        {
            track.Points.Sort((p1, p2) => p1.Time.CompareTo(p2.Time));
            return track;
        }).ToList();

        BCaster.Broadcast($"Preprocessed {preprocessedTracks.Count} tracks", Observation.LogLevel.Debug);

        foreach (ISongEntry song in songs)
        {
            // Filter tracks that are within the song's time frame
            List<GpsTrack> relevantTracks = preprocessedTracks
                .Where(track => song.WithinTimeFrame(track.Start, track.End))
                .ToList();

            BCaster.Broadcast($"Found {relevantTracks.Count} relevant tracks for '{song.ToString()}'", Observation.LogLevel.Debug);

            List<Task> tasks = new();
            foreach (GpsTrack track in relevantTracks)
            {
                tasks.Add(Task.Run(() =>
                {
                    // Use binary search to find the closest point
                    if (!TryFindClosestPoint(track.Points, song.Time, out IGpsPoint? bestPoint, out double accuracy))
                    {
                        BCaster.BroadcastError(new Exception($"Unable to find close point for {song}, its accuracy ({accuracy}) to the closest point is higher than tolerated ceiling ({MaximumAbsAccuracy})!"));
                        return;
                    }
                    else if (bestPoint == null)
                    {
                        BCaster.BroadcastError(new Exception($"Closest point is null for {song}!"));
                        return;
                    }

                    SongPoint pair = new(index, song, bestPoint, track.Track);

                    BCaster.Broadcast($"{pair.ToString()}", Observation.LogLevel.Pair);

                    lock (pairs)
                    {
                        pairs.Add(pair);
                        index++;
                    }
                }));
            }

            Task[] pointsToPair = tasks.ToArray();

            BCaster.Broadcast($"Waiting for {pointsToPair.Length} pairs to be created for '{song.ToString()}'", Observation.LogLevel.Debug);

            Task.WaitAll(pointsToPair);

            BCaster.Broadcast($"{pointsToPair.Length} pairs created for '{song.ToString()}'", Observation.LogLevel.Debug);
        }

        return pairs;
    }

    private static bool TryFindClosestPoint(List<IGpsPoint> points, DateTimeOffset songTime, out IGpsPoint? nearestPoint, out double accuracy)
    {
        nearestPoint = null;

        if (points == null || points.Count == 0)
        {
            throw new Exception("The points list cannot be null or empty.");
        }

        int left = 0;
        int right = points.Count - 1;

        // Perform binary search to narrow down the closest point by time
        while (left < right)
        {
            int mid = left + (right - left) / 2;

            if (points[mid].Time < songTime)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }

        // At this point, left is the smallest index with points[left].Time >= songTime
        if (left == 0)
        {
            nearestPoint = points[0];
            accuracy = SongPoint.DisplacementCalculatorAbs(points[0].Time, songTime);
            return true;
        }

        // If the song time is greater than the last point, return the last point
        if (left >= points.Count)
        {
            nearestPoint = points[points.Count - 1];
            accuracy = SongPoint.DisplacementCalculatorAbs(points[points.Count - 1].Time, songTime);
            return true;
        }

        double leftDisplacement = SongPoint.DisplacementCalculatorAbs(points[left].Time, songTime);
        double rightDisplacement = SongPoint.DisplacementCalculatorAbs(points[left - 1].Time, songTime);

        nearestPoint = leftDisplacement < rightDisplacement ? points[left] : points[left - 1];
        accuracy = Math.Min(leftDisplacement, rightDisplacement);

        if (MaximumAbsAccuracy != null && accuracy > MaximumAbsAccuracy)
        {
            return false;
        }

        return true;
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
