// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace SpotifyGPX.Gpx;

public partial class GPX
{
    private static XElement TrackManager()
    {
        List<XElement> tracks = document.Descendants(Namespace + "trk").ToList();

        XElement selectedTrack;

        if (tracks.Count > 1)
        {
            // If there are multiple <trk> elements, prompt the user to select one
            Console.WriteLine("Multiple <trk> elements found. Please choose the track you want to use:");
            for (int i = 0; i < tracks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tracks[i].Element(Namespace + "name")?.Value ?? $"Track {i}"}");
            }

            int selectedTrackIndex;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out selectedTrackIndex) && selectedTrackIndex >= 1 && selectedTrackIndex <= tracks.Count)
                {
                    break;
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

        return selectedTrack;
    }
}
