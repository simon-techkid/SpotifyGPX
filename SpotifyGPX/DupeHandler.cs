// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// Handle duplicate coordinate placements by shifting them to other locations.
/// </summary>
public partial class DupeHandler
{
    private List<SongPoint> Pairs { get; }

    /// <summary>
    /// Create a handler for duplicate positions.
    /// </summary>
    /// <param name="pairs">A list of pairs to be searched for duplicate positions.</param>
    public DupeHandler(List<SongPoint> pairs) => Pairs = pairs;

    /// <summary>
    /// Run prediction (dupe calculation) using the pairs list this DupeHandler was initialized with.
    /// </summary>
    /// <param name="autoPredict">If true, do not ask user for pair indexes of dupes.</param>
    /// <returns>A list of pairs with the formerly duplicate positioned pairs shifted.</returns>
    public List<SongPoint> GetDupes(bool autoPredict)
    {
        if (Pairs.Count < MinimumMatchingCoords)
        {
            Console.WriteLine($"[PRED] Point prediction cannot be run when there are less than {MinimumMatchingCoords} pairs");
            return Pairs;
        }

        Console.WriteLine($"[PRED] Autopredict is {(autoPredict == true ? "enabled, automatically predicting" : "disabled, you will be prompted")}");
        return PredictPoints(autoPredict);
    }

    /// <summary>
    /// Find and handle duplicate coordinate positions.
    /// </summary>
    /// <param name="autoPredict">If true, shift all duplicate positions regardless of user intent. If false, ask the user for index ranges of duplicates.</param>
    /// <returns>The original pairs list, with duplicate coordinates shifted.</returns>
    private List<SongPoint> PredictPoints(bool autoPredict)
    {
        Console.WriteLine("[PRED] Scanning for duplicate entries:");
        var dupes = GroupDuplicates();
        PrintDuplicates(dupes);

        List<(int startIndex, int endIndex)> specDupes = new();

        if (autoPredict == true)
        {
            specDupes = GetAllDupes();
        }
        else
        {
            specDupes = GetDupesFromUser();
        }

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

    /// <summary>
    /// Get the start and end indexes of pairs (sharing the same coordinates) in a series.
    /// </summary>
    /// <returns>A list containing each duplicate coordinate group's start and end index as an integer.</returns>
    private List<(int, int)> GetAllDupes()
    {
        var dupes = GroupDuplicates();

        return dupes.Select(dupe => (dupe.First().Index, dupe.Last().Index)).ToList();
    }

    /// <summary>
    /// Ask the user to provide start and end indexes for a series of pairs.
    /// </summary>
    /// <returns>A list containing each duplicate coordinate group's start and end index as an integer.</returns>
    private List<(int, int)> GetDupesFromUser()
    {
        List<(int startIndex, int endIndex)> specDupes = new();

        bool isValidInput = false;

        Console.Write("[DUPE] Write the start and end indexes (separated by a dash) of each of your dupes, with dupes separated by commas: ");
        while (!isValidInput)
        {
            string dupeDefinition = Console.ReadLine() ?? string.Empty;

            string[] selectedDupes = dupeDefinition.Split(",");

            foreach (string dupe in selectedDupes)
            {
                int maximumAllowedIndex = Pairs.Count - 1;

                string[] indexes = dupe.Split("-");

                if (indexes.Length == 2 && int.TryParse(indexes[0], out int startIndex) && int.TryParse(indexes[1], out int endIndex))
                {
                    if (startIndex < 0 || startIndex >= Pairs.Count)
                    {
                        Console.WriteLine($"Invalid startIndex: {startIndex}. Must be between 0 and {maximumAllowedIndex}.");
                        isValidInput = false;
                        break;
                    }
                    else if (endIndex < 0 || endIndex >= Pairs.Count)
                    {
                        Console.WriteLine($"Invalid endIndex: {endIndex}. Must be between 0 and {maximumAllowedIndex}.");
                        isValidInput = false;
                        break;
                    }
                    else if (endIndex - startIndex == 0)
                    {
                        Console.WriteLine($"Invalid range: {startIndex}-{endIndex}. Range must include at least one element.");
                        isValidInput = false;
                        break;
                    }

                    specDupes.Add((startIndex, endIndex));
                    isValidInput = true; // Input is valid only if all conditions pass
                }
                else
                {
                    Console.WriteLine($"Invalid input: '{dupe}'. Please enter start and end indexes separated by a dash.");
                    isValidInput = false;
                    break;
                }
            }

            if (isValidInput)
            {
                break;
            }
        }

        return specDupes;
    }

    /// <summary>
    /// Get the next coordinate after a specified index in a pairs list.
    /// </summary>
    /// <param name="pairs">The list of pairs.</param>
    /// <param name="endIndex">The index of the last pair in a duplicate.</param>
    /// <returns>The coordinate following the last duplicate, the next unique coordinate.</returns>
    private static Coordinate GetNextUniqueCoord(List<SongPoint> pairs, int endIndex)
    {
        int nextIndex = endIndex + 1;
        int lastIndex = pairs.Count - 1;
        return pairs[nextIndex < lastIndex ? nextIndex : endIndex].Point.Location;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parsedDupes"></param>
    /// <returns></returns>
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

    /// <summary>
    /// An object representing a series of pairs' matching coordinates.
    /// </summary>
    private readonly struct Dupe
    {
        /// <summary>
        /// Creates a Dupe object, representing a cluster of pairs with matching coordinates.
        /// </summary>
        /// <param name="startIndex">The index of the first pair (in a list) with a shared coordinate.</param>
        /// <param name="endIndex">The index of the last pair (in a list) with a shared coordinate.</param>
        /// <param name="first">The first coordinate of the dupe, the coordinate shared among all pairs in this group.</param>
        /// <param name="next">The next unique coordinate, taken from the pair just after this dupe.</param>
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
    private List<IGrouping<Coordinate, SongPoint>> GroupDuplicates()
    {
        // Create list of grouped duplicate coordinate values from final points list
        return Pairs
            .Where(pair => pair.Origin.Type == TrackType.Gps) // Filter out non-GPX journey dupes
            .GroupBy(p => p.Point.Location) // Group them by matching locations
            .Where(group => group.Count() >= MinimumMatchingCoords) // Filter out groups not having enough matching coords
            .ToList(); // Send the resulting dupes to a list
    }

    /// <summary>
    /// Prints each group of shared-coordinate pairs to the console.
    /// </summary>
    /// <param name="dupes">An IGrouping, with TKey as Coordinate (the shared coordinate), and TElement as SongPoint (the SongPoint implicated).</param>
    private static void PrintDuplicates(List<IGrouping<Coordinate, SongPoint>> dupes)
    {
        foreach (var dupe in dupes)
        {
            string g = string.Join(", ", dupe.Select(s => $"{s.Song.Song_Name} ({s.Index})"));
            Console.WriteLine($"[DUPE {dupes.IndexOf(dupe)}] {g}");
        }
    }

    /// <summary>
    /// Calculates an array of a specified number of equally-spaced coordinates (in a line) between two given coordinates.
    /// </summary>
    /// <param name="start">The start point of the line array.</param>
    /// <param name="end">The end point of the line array.</param>
    /// <param name="dupeCount">The number of points to calculate between the start and end points.</param>
    /// <returns></returns>
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
