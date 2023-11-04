// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace SpotifyGPX.Gpx;

public partial class GPX
{
    private static void TrackManager()
    {
        List<XElement> tracks = document.Descendants(Namespace + "trk").ToList();

        XElement selectedTrack;

        if (tracks.Count > 1)
        {
            // If there are multiple <trk> elements, prompt the user to select one
            Console.WriteLine("[TRAK] Multiple GPX tracks found:");
            for (int i = 0; i < tracks.Count; i++)
            {
                Console.WriteLine($"[TRAK] [{i + 1}] {tracks[i].Element(Namespace + "name")?.Value ?? $"Track {i}"}");
            }

            Console.WriteLine("[TRAK] [A] All Tracks");

            Console.Write("[TRAK] Please choose the track you want to use: ");

            int selectedTrackIndex;
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out selectedTrackIndex) && selectedTrackIndex >= 1 && selectedTrackIndex <= tracks.Count)
                {
                    break;
                }
                else if (input == "A")
                {
                    ParseAll(tracks);
                    return;                   
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid track number.");
                }
            }

            // Select the user-chosen <trk> element
            selectedTrack = tracks[selectedTrackIndex - 1];
        }
        else
        {
            selectedTrack = tracks[0];
        }

        List<XElement> finalSelect = new()
        {
            selectedTrack
        };

        ParseAll(finalSelect);

        return;
    }
}
