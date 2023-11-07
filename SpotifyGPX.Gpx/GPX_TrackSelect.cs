// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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

            Console.WriteLine("[TRAK] [A] All Tracks (Only include songs played during tracks)");
            Console.WriteLine("[TRAK] [B] All Tracks (Include songs played in gaps between tracks)");

            Console.Write("[TRAK] Please choose the track you want to use: ");

            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out int selectedTrackIndex) && selectedTrackIndex >= 1 && selectedTrackIndex <= tracks.Count)
                {
                    // Select the user-chosen <trk> element
                    selectedTrack = tracks[selectedTrackIndex - 1];

                    break;
                }
                else if (input == "A")
                {
                    ParseTracks(tracks);
                    return;
                }
                else if (input == "B")
                {
                    selectedTrack = new XElement("combined", tracks);
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid track number.");
                }
            }
        }
        else
        {
            selectedTrack = tracks[0];
        }

        List<XElement> finalSelect = new()
        {
            selectedTrack
        };

        ParseTracks(finalSelect);

        return;
    }
}
