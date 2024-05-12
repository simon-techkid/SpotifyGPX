// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public sealed class SongTest : RandomSongBase
{
    protected override ParseSongsDelegate ParseSongsMethod => ParseSongs;
    protected override DateOnly GeneratorStartDate => DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(5));
    protected override DateOnly GeneratorEndDate => DateOnly.FromDateTime(DateTime.Now);
    protected override TimeOnly DayStartTime => new(PlaybackStartHour, 0);
    protected override TimeOnly DayEndTime => new(PlaybackEndHour, 0);
    private const int PlaybackStartHour = 7; // Music playback begin before drive
    private const int PlaybackEndHour = 23; // Music playback end after drive
    protected override TimeSpan TimeZone => new(0, 0, 0); // simulate Spotify UTC timing
    protected override int IntervalSecondsMin => 120; // simulate 2 minute minimum song length
    protected override int IntervalSecondsMax => 330; // simulate 5.5 minute maximum song length
    protected override int UniqueArtistsCount => 10; // simulate 10 unique artists
    protected override int UniqueSongsCount => 100; // simulate 100 unique songs across 10 artists

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
                Description = "A random song",
                Index = index,
                Song_Artist = $"Artist{random.Artist}",
                Song_Name = $"Song{random.Song}",
                FriendlyTime = random.Time
            };
        }).ToList();
    }
}
