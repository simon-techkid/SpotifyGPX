// SpotifyGPX by Simon Field

using SpotifyGPX.Api;
using SpotifyGPX.Broadcasting;
using SpotifyGPX.Input;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Pairings;

public class PairingsFromSongsAndPoints : PairingsFactory
{
    private string InputSongs { get; }
    private string InputGps { get; }
    private bool GrabApiData { get; }

    public PairingsFromSongsAndPoints(string inputSongs, string inputGps, bool grabApiData, bool pointPredict, bool autoPredictPoints, StringBroadcaster bcast) : base(bcast, pointPredict, autoPredictPoints)
    {
        InputSongs = inputSongs;
        InputGps = inputGps;
        GrabApiData = grabApiData;
    }

    protected override string PairingsSetName => Path.GetFileNameWithoutExtension(InputGps);

    public override PairingsHandler GetHandler()
    {
        PairingsHandler pairer = GetDupeOrRegHandler();

        List<GpsTrack> tracks = new();
        List<ISongEntry> songs = new();

        using (InputHandler input = new(InputSongs, InputGps, BCaster.Clone()))
        {
            tracks = input.GetSelectedTracks();
            songs = input.GetFilteredSongs(tracks);
            //songs = input.GetAllSongs(); // Unfiltered run
        }

        if (GrabApiData)
            songs = new EntryMatcher(songs).MatchEntries();

        pairer.CalculatePairings(songs, tracks);
        return pairer;
    }
}
