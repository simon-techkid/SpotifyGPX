// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// Handle duplicate coordinate placements by shifting them to other locations.
/// </summary>
public abstract partial class DupeHandler : PairingsHandler
{
    public override string Name { get; }

    protected DupeHandler(string name, StringBroadcaster bcast) : base(bcast)
    {
        Name = name;
    }

    public override void CalculatePairings(List<ISongEntry> s, List<GpsTrack> t)
    {
        CalculatePairings(PairPoints(s, t));
    }

    public override void CalculatePairings(List<SongPoint> pairs)
    {
        Pairs = pairs;
        Pairs = GetDupes();
    }

    private List<SongPoint> GetDupes()
    {
        if (Pairs.Count < MinimumMatchingCoords)
        {
            BCaster.BroadcastError(new Exception($"Point prediction cannot be run when there are less than {MinimumMatchingCoords} pairs"));
            return Pairs;
        }

        return PredictPoints();
    }

    protected abstract List<(int, int)> GetDupeIndexes();

    private List<SongPoint> PredictPoints()
    {
        BCaster.Broadcast("Scanning for duplicate entries:");
        var dupes = GroupDuplicates();
        PrintDuplicates(dupes);

        List<(int startIndex, int endIndex)> specDupes = GetDupeIndexes();

        List<Dupe> parsedDupes = specDupes.Select(dupe =>
        {
            int startIndex = dupe.startIndex;
            int endIndex = dupe.endIndex;

            Coordinate firstCoordOfDupe = Pairs[startIndex].Point.Location;
            Coordinate nextUniqueCoord = GetNextUniqueCoord(Pairs, endIndex);

            return new Dupe(startIndex, endIndex, firstCoordOfDupe, nextUniqueCoord);
        }).ToList();

        return ApplyPredictions(parsedDupes);
    }

    private static Coordinate GetNextUniqueCoord(List<SongPoint> pairs, int endIndex)
    {
        int nextIndex = endIndex + 1;
        int lastIndex = pairs.Count - 1;
        return pairs[nextIndex < lastIndex ? nextIndex : endIndex].Point.Location;
    }

    private List<SongPoint> ApplyPredictions(List<Dupe> parsedDupes)
    {
        return Pairs
            .Select((originalPair, index) =>
            {
                // Get the dupe for this pair
                Dupe matchingDupe = parsedDupes.FirstOrDefault(dupe => index >= dupe.StartIndex && index <= dupe.EndIndex);

                // If this pair is part of a dupe,
                if (!matchingDupe.Equals(default(Dupe)))
                {
                    Coordinate[] newCoords = matchingDupe.Coords; // Get the array of predicted coordinates
                    int relativeIndex = index - matchingDupe.StartIndex; // Calculate the relative index within the duplicate range
                    if (relativeIndex >= 0 && relativeIndex < newCoords.Length)
                    {
                        // If the index of this dupe is less than the length of the array of new coordinates,
                        return new SongPoint(originalPair, newCoords[relativeIndex], relativeIndex); // Create new pair from old pair (with new coords)
                    }
                }

                // If this pair is not part of a dupe,
                return originalPair; // Return the original pair
            })
            .ToList();
    }

    private readonly struct Dupe
    {
        public Dupe(int startIndex, int endIndex, Coordinate first, Coordinate next)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            FirstCoordOfDupe = first;
            NextUniqueCoord = next;
        }

        /// <summary>
        /// The index of the first pair (in a list) with a shared coordinate.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// The index of the last pair (in a list) with a shared coordinate.
        /// </summary>
        public int EndIndex { get; }

        /// <summary>
        /// The first coordinate of the dupe, the coordinate shared among all pairs in this group.
        /// </summary>
        private Coordinate FirstCoordOfDupe { get; }

        /// <summary>
        /// The next unique coordinate, taken from the pair just after this dupe.
        /// </summary>
        private Coordinate NextUniqueCoord { get; }

        /// <summary>
        /// An array of intermediate coordinates at equal distances between the start position of the duplicate positions and the next unqiue position.
        /// </summary>
        public Coordinate[] Coords
        {
            get
            {
                int count = EndIndex - StartIndex + 1;
                return GetIntermediates(FirstCoordOfDupe, NextUniqueCoord, count);
            }
        }
    }

    /// <summary>
    /// Finds and groups pairs sharing the same coordinate.
    /// </summary>
    /// <returns>An IGrouping, with TKey as Coordinate (the shared coordinate), and TElement as SongPoint (the SongPoint implicated).</returns>
    protected List<IGrouping<Coordinate, SongPoint>> GroupDuplicates()
    {
        // Create list of grouped duplicate coordinate values from final points list
        return Pairs
            .Where(pair => pair.Origin.Type == TrackType.Gps) // Filter out non-GPS journey dupes
            .GroupBy(p => p.Point.Location) // Group them by matching locations
            .Where(group => group.Count() >= MinimumMatchingCoords) // Filter out groups not having enough matching coords
            .ToList(); // Send the resulting dupes to a list
    }

    private void PrintDuplicates(List<IGrouping<Coordinate, SongPoint>> dupes)
    {
        foreach (var dupe in dupes)
        {
            string g = string.Join(", ", dupe.Select(s => $"{s.Song.Song_Name} ({s.Index})"));
            BCaster.Broadcast($"[{dupes.IndexOf(dupe)}] {g}");
        }
    }

    private static Coordinate[] GetIntermediates(Coordinate start, Coordinate end, int dupeCount)
    {
        // Parse start coordinate and end coordinate to lat and lon doubles
        // For each dupe, determine its equidistant point
        var intermediatePoints = new Coordinate[dupeCount];

        for (int iteration = 0; iteration < dupeCount; iteration++)
        {
            // Determine the average for this iteration based on the number of dupes between the start and end points
            double average = (double)iteration / dupeCount;

            // Determine the intermediate lat/lon based on the start/end point average
            Coordinate intermediatePoint = start + (end - start) * average;

            // Replace the list entry with the intermediate point
            intermediatePoints[iteration] = intermediatePoint;
        }
        return intermediatePoints;
    }
}
