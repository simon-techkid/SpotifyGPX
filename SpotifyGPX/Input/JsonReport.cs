using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX.Input;

public class JsonReport : ISongInput, IGpsInput
{
    private List<JObject> JsonTracks { get; }
    private List<SpotifyEntry> AllSongs { get; }
    private List<GPXTrack> AllTracks { get; }

    public JsonReport(string path)
    {
        JsonTracks = Deserialize(path);
        (List<GPXTrack> tracks, List<SpotifyEntry> songs) = GetFromJObject();
        AllTracks = tracks;
        AllSongs = songs;

        Console.WriteLine($"SongCount: {Count}, TrackCount: {TrackCount}, PointCount: {PointCount}");
    }


    public (List<GPXTrack>, List<SpotifyEntry>) GetFromJObject()
    {
        List<GPXTrack> tracks = new();
        List<SpotifyEntry> songs = new();

        foreach (JObject track in JsonTracks)
        {
            JObject trackInfo = (JObject)track["TrackInfo"];
            JArray pairsArray = (JArray)track[trackInfo["Name"].ToString()];
            TrackInfo ti = track["TrackInfo"].ToObject<TrackInfo>();

            List<GPXPoint> points = pairsArray.Select(pair => pair["Point"].ToObject<GPXPoint>()).ToList();

            GPXTrack t = new(ti.Index, ti.Name, ti.Type, points);
            tracks.Add(t);

            songs.AddRange(pairsArray.Select(pair => pair["Song"].ToObject<SpotifyEntry>()).ToList());
        }

        return (tracks, songs);
    }

    public int Count => AllSongs.Count;
    public int TrackCount => JsonTracks.Count;
    public int PointCount => AllTracks.Select(track => track.Points.Count).Sum();

    public List<GPXTrack> GetAllTracks()
    {
        return AllTracks;
    }

    public List<SpotifyEntry> GetAllSongs()
    {
        return AllSongs;
    }

    private static List<JObject> Deserialize(string jsonFilePath)
    {
        var serializer = new JsonSerializer();

        using var fileStream = File.OpenRead(jsonFilePath);
        using var jsonReader = new JsonTextReader(new StreamReader(fileStream));
        List<JObject> tracks = new();

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                var json = serializer.Deserialize<JObject>(jsonReader);
                if (json != null)
                {
                    tracks.Add(json);
                }
            }
        }

        return tracks;
    }


}
