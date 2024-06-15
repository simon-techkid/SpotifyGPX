// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public sealed partial class SongTest : RandomSongBase
{
    public override List<ISongEntry> ParseSongsMethod() => ParseSongs();
    public override List<ISongEntry> FilterSongsMethod() => FilterSongs();
    protected override DateOnly GeneratorStartDate => DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(DaysPriorToTodayToGenerate));
    protected override DateOnly GeneratorEndDate => DateOnly.FromDateTime(DateTime.Now);
    private int PlaybackStartHour => RandomGen.Next(PlaybackMinStartHour, PlaybackMaxStartHour);
    private int PlaybackStartMinute => RandomGen.Next(PlaybackMinStartMinute, PlaybackMaxStartMinute);
    private int PlaybackEndHour => RandomGen.Next(PlaybackMinEndHour, PlaybackMaxEndHour);
    private int PlaybackEndMinute => RandomGen.Next(PlaybackMinEndMinute, PlaybackMaxEndMinute);
    protected override TimeOnly DayStartTime => new(PlaybackStartHour, PlaybackStartMinute);
    protected override TimeOnly DayEndTime => new(PlaybackEndHour, PlaybackEndMinute);
    protected override TimeSpan TimeZone => new(0, 0, 0); // simulate Spotify UTC timing
    protected override int IntervalSecondsMin => 120; // simulate 2 minute minimum song length
    protected override int IntervalSecondsMax => 330; // simulate 5.5 minute maximum song length
    protected override int UniqueArtistsCount => ArtistsCount; // simulate 10 unique artists
    protected override int UniqueSongsCount => SongsCount; // simulate 100 unique songs across 10 artists

    public SongTest() : base()
    {
    }

    private List<ISongEntry> ParseSongs()
    {
        List<RandomSong> random = ZipAll();

        return random.Select((random, index) =>
        {
            return (ISongEntry)new GenericEntry
            {
                Description = "A randomly generated song",
                Index = index,
                Song_Artist = $"Artist{random.Artist}",
                Song_Name = $"Song{random.Song}",
                FriendlyTime = random.Time,
                CurrentUsage = TimeUsage.Start,
                CurrentInterpretation = TimeInterpretation.Start
            };
        }).ToList();
    }

    private List<ISongEntry> FilterSongs()
    {
        return AllSongs;
    }
}
