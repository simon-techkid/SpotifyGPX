// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

public class DupeHandler
{
    private List<SongPoint> Pairs { get; }

    public DupeHandler(List<SongPoint> pairs) => Pairs = pairs;

    public List<SongPoint> GetDupes(bool autoPredict)
    {
        Console.WriteLine($"[PRED] Autopredict is {(autoPredict == true ? "enabled, automatically predicting" : "disabled, you will be prompted")}");
        return PredictPoints(autoPredict);
    }

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

    private List<(int, int)> GetAllDupes()
    {
        var dupes = GroupDuplicates();

        return dupes.Select(dupe => (dupe.First().Index, dupe.Last().Index)).ToList();
    }

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
                Dupe matchingDupe = parsedDupes.FirstOrDefault(dupe => index >= dupe.StartIndex && index <= dupe.EndIndex);
                if (!matchingDupe.Equals(default(Dupe)))
                {
                    Coordinate[] newCoords = matchingDupe.Coords;
                    int relativeIndex = index - matchingDupe.StartIndex; // Calculate the relative index within the duplicate range
                    if (relativeIndex >= 0 && relativeIndex < newCoords.Length)
                    {
                        return new SongPoint(originalPair, newCoords[relativeIndex]); // Create new pair from old pair (with new coords)
                    }
                }
                return originalPair; // Not a predicted point, return the original pair
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

        public int StartIndex { get; }
        public int EndIndex { get; }
        private Coordinate FirstCoordOfDupe { get; }
        private Coordinate NextUniqueCoord { get; }
        public Coordinate[] Coords
        {
            get
            {
                int count = EndIndex - StartIndex + 1;
                return GetIntermediates(FirstCoordOfDupe, NextUniqueCoord, count);
            }
        }
    }

    private List<IGrouping<Coordinate, SongPoint>> GroupDuplicates()
    {
        // Create list of grouped duplicate coordinate values from final points list
        return Pairs
            .GroupBy(p => p.Point.Location)
            .Where(group => group.Count() >= 2 && group.Any(sp => sp.Origin.Type == TrackType.GPX))
            .ToList();
    }

    private static void PrintDuplicates(List<IGrouping<Coordinate, SongPoint>> dupes)
    {
        foreach (var dupe in dupes)
        {
            string g = string.Join(", ", dupe.Select(s => $"{s.Song.Song_Name} ({s.Index})"));
            Console.WriteLine($"[DUPE {dupes.IndexOf(dupe)}] {g}");
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
