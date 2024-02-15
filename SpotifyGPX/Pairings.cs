// SpotifyGPX by Simon Field

using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpotifyGPX;

public class Pairings
{
    private static double? MaximumAbsAccuracy => null; // Greatest accepted error (in seconds) between song and point time (null = allow all pairings regardless of accuracy)

    public Pairings(List<SpotifyEntry> s, List<GPXTrack> t) => Pairs = PairPoints(s, t);
    private readonly List<SongPoint> Pairs;

    private static List<SongPoint> PairPoints(List<SpotifyEntry> songs, List<GPXTrack> gpxTracks)
    {
        // Correlate Spotify entries with the nearest GPX points

        int index = 0; // Index of the pairing

        return gpxTracks // For each GPX track
        .SelectMany(gpxTrack => songs // Get the list of SpotifyEntries
        .Where(spotifyEntry => spotifyEntry.WithinTimeFrame(gpxTrack.Start, gpxTrack.End)) // If the SpotifyEntry falls within the boundaries of the track
        .Select(spotifyEntry => // Select the Spotify entry if it falls in range of the GPX track
            {
                SongPoint pair = gpxTrack.Points
                .Select(point => new SongPoint(index, spotifyEntry, point, gpxTrack.Track)) // For each point in the track's point list,
                .OrderBy(pair => pair.AbsAccuracy) // Order the points by proximity between point and song
                .First(); // Closest accuracy wins

                Console.WriteLine(pair.ToString());

                index++; // Add to the index of all pairings regardless of track

                return pair;
            })
        )
        .Where(pair => MaximumAbsAccuracy == null || pair.AbsAccuracy <= MaximumAbsAccuracy) // Only create pairings with accuracy equal to or below max allowed accuracy
        .ToList();
    }

    public void Save(Formats format, string sourceGpxName)
    {
        // Parse the Pairs in this Pairings object to the specified format, according to user-provided arguments
        OutputHandler fmat = new(Pairs);

        // Save the handled format as specified by the provided file name prefix
        fmat.Save(format, sourceGpxName);
    }

    public void WriteCounts()
    {
        WriteCounts(pair => pair.Origin, "track", "tracks"); // Write # of pairs per track
        WriteCounts(pair => pair.Origin.Type, "type", "types"); // Write # of pairs in each type of track (GPX, Gap, Combined)
        WriteCounts(pair => pair.Song.Spotify_Country, "country", "countries"); // Write # of pairs in each country
    }

    private void WriteCounts<T>(Func<SongPoint, T> groupingSelector, string nameSingular, string nameMultiple)
    {
        var groupedPairs = Pairs.GroupBy(groupingSelector); // Group all the song-point pairs by the specified selector
        string countsJoined = string.Join(", ", groupedPairs.Select(group => $"{group.Count()} ({group.Key})"));
        int groupCount = groupedPairs.Count();
        string objName = groupCount > 1 ? nameMultiple : nameSingular;

        Console.WriteLine($"[PAIR] Paired {Pairs.Count} songs and points from {groupCount} {objName}: {countsJoined}");
    }

    public void WriteAverages()
    {
        WriteAverages(pair => pair.Origin.Type, "track type", "track types"); // Calculate Accuracies by track type (GPX, Gap, Combined)
        WriteAverages(pair => pair.Origin, "track", "tracks"); // Calculate Accuracies by track
    }

    private void WriteAverages<T>(Func<SongPoint, T> groupingSelector, string nameSingular, string nameMultiple)
    {
        var groupedPairs = Pairs.GroupBy(groupingSelector); // Group all the song-point pairs by the specified selector
        string accuraciesJoined = string.Join(", ", groupedPairs.Select(group => $"{Math.Round(group.Average(pair => pair.AbsAccuracy))}s ({group.Key})"));
        int groupCount = groupedPairs.Count();
        string objName = groupCount > 1 ? nameMultiple : nameSingular;

        Console.WriteLine($"[PAIR] Average Accuracy for {groupCount} {objName}: {accuraciesJoined}");
    }
}