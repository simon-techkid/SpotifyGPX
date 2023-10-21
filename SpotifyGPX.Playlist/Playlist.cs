// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using SpotifyGPX.Options;

namespace SpotifyGPX.Playlist
{
    public class XSPF
    {
        public static XmlDocument CreatePlist(List<SpotifyEntry> tracks, string plistFile)
        {
            // Create a new XML document
            XmlDocument document = new();

            // Create the XML header
            XmlNode header = document.CreateXmlDeclaration("1.0", "utf-8", null);
            document.AppendChild(header);

            // Create the XSPF header
            XmlElement XSPF = document.CreateElement("playlist");
            document.AppendChild(XSPF);

            // Add XSPF header attributes
            XSPF.SetAttribute("version", "1.0");
            XSPF.SetAttribute("xmlns", "http://xspf.org/ns/0/");

            // Set the name of the XSPF playlist to the name of the file
            XmlElement name = document.CreateElement("name");
            name.InnerText = Path.GetFileNameWithoutExtension(plistFile);
            XSPF.AppendChild(name);

            // Set the title of the XSPF playlist to the name of the file
            XmlElement creator = document.CreateElement("creator");
            creator.InnerText = "SpotifyGPX";
            XSPF.AppendChild(creator);

            // Create the trackList header
            XmlElement trackList = document.CreateElement("trackList");
            XSPF.AppendChild(trackList);

            foreach (SpotifyEntry entry in tracks)
            {
                // Create track for each song
                XmlElement track = document.CreateElement("track");
                trackList.AppendChild(track);

                // Set the creator of the track to the song artist
                XmlElement artist = document.CreateElement("creator");
                artist.InnerText = entry.Song_Artist;
                track.AppendChild(artist);

                // Set the title of the track to the song name
                XmlElement title = document.CreateElement("title");
                title.InnerText = entry.Song_Name;
                track.AppendChild(title);

                // Set the annotation of the song to the end time
                XmlElement annotation = document.CreateElement("annotation");
                annotation.InnerText = entry.Time_End.ToString(Formats.gpxPointTimeInp);
                track.AppendChild(annotation);

                // Set the duration of the song to the amount of time it was listened to
                XmlElement duration = document.CreateElement("duration");
                duration.InnerText = entry.Time_Played;
                track.AppendChild(duration);
            }

            return document;
        }
    }
}
