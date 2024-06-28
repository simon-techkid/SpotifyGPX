// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;

namespace SpotifyGPX.Pairings;

public class DupeHandlerManual : DupeHandler
{
    public DupeHandlerManual(string name, StringBroadcaster bcast) : base(name, bcast) { }

    protected override List<(int, int)> GetDupeIndexes()
    {
        List<(int startIndex, int endIndex)> specDupes = new();

        bool isValidInput = false;

        BCaster.Broadcast("Write the start and end indexes (separated by a dash) of each of your dupes, with dupes separated by commas: ");
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
                        BCaster.BroadcastError(new Exception($"Invalid startIndex: {startIndex}. Must be between 0 and {maximumAllowedIndex}."));
                        isValidInput = false;
                        break;
                    }
                    else if (endIndex < 0 || endIndex >= Pairs.Count)
                    {
                        BCaster.BroadcastError(new Exception($"Invalid endIndex: {endIndex}. Must be between 0 and {maximumAllowedIndex}."));
                        isValidInput = false;
                        break;
                    }
                    else if (endIndex - startIndex == 0)
                    {
                        BCaster.BroadcastError(new Exception($"Invalid range: {startIndex}-{endIndex}. Range must include at least one element."));
                        isValidInput = false;
                        break;
                    }

                    specDupes.Add((startIndex, endIndex));
                    isValidInput = true; // Input is valid only if all conditions pass
                }
                else
                {
                    BCaster.BroadcastError(new Exception($"Invalid input: '{dupe}'. Please enter start and end indexes separated by a dash."));
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
}
