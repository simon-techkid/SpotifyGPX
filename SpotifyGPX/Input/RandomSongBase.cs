// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public abstract class RandomSongBase : RandomInputBase<RandomSong>, ISongInput
{
    protected RandomSongBase() : base()
    {
    }

    public int SourceSongCount => 0;
    public int ParsedSongCount => AllSongs.Count;

    public abstract ISongInput.ParseSongsDelegate ParseSongsMethod { get; }
    public abstract ISongInput.FilterSongsDelegate FilterSongsMethod { get; }
    protected List<ISongEntry> AllSongs => ParseSongsMethod();

    protected override List<RandomSong> ZipAll()
    {
        List<DateTimeOffset> playDates = GenerateDateTimeOffsets()
            .Where(TimeCheck)
            .ToList();

        IEnumerable<string> songs = ExtendCollection(GenerateNumbers(UniqueSongsCount), playDates.Count).Shuffle();
        IEnumerable<string> artists = ExtendCollection(GenerateNumbers(UniqueArtistsCount), playDates.Count).Shuffle();

        return songs.Zip(artists, (song, artist) => new { song, artist })
            .Zip(playDates, (pair, date) => new RandomSong { Song = pair.song, Artist = pair.artist, Time = date })
            .ToList();
    }

    /// <summary>
    /// The length of generated GUID strings.
    /// This value is the length of the base64 string representation of a GUID.
    /// This value may be overriden, but the default value is 8.
    /// </summary>
    protected virtual int StringLength => 8;

    /// <summary>
    /// The floor (minimum) random number to generate.
    /// </summary>
    protected virtual int MinRandomNumber => 0;

    /// <summary>
    /// The ceiling (maximum) random number to generate.
    /// </summary>
    protected virtual int MaxRandomNumber => 100;

    /// <summary>
    /// The minimum length of a randomized song.
    /// This value is the floor of a random song's length in seconds.
    /// </summary>
    protected abstract int IntervalSecondsMin { get; }

    /// <summary>
    /// The maximum length of a randomized song.
    /// This value is the ceiling of a random song's length in seconds.
    /// </summary>
    protected abstract int IntervalSecondsMax { get; }

    /// <summary>
    /// The count of unique artists to generate.
    /// </summary>
    protected abstract int UniqueArtistsCount { get; }

    /// <summary>
    /// The count of unique songs to generate.
    /// </summary>
    protected abstract int UniqueSongsCount { get; }

    protected override IEnumerable<DateTimeOffset> GenerateDateTimeOffsets()
    {
        if (IsValidTimes() == false)
            throw new ArgumentException("Invalid start and end times");

        TimeSpan minInterval = TimeSpan.FromSeconds(IntervalSecondsMin);
        TimeSpan maxInterval = TimeSpan.FromSeconds(IntervalSecondsMax);

        DateTimeOffset current = First;

        while (current < Last)
        {
            yield return current;

            // Generate a random interval between minInterval and maxInterval
            double minTotalSeconds = minInterval.TotalSeconds;
            double maxTotalSeconds = maxInterval.TotalSeconds;
            double randomSeconds = RandomGen.NextDouble() * (maxTotalSeconds - minTotalSeconds) + minTotalSeconds;
            TimeSpan interval = TimeSpan.FromSeconds(randomSeconds);

            // Update current with the new interval
            current = current.Add(interval);
        }
    }

    private IEnumerable<string> GenerateNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return GenerateNumber().ToString();
        }
    }

    private int GenerateNumber()
    {
        return RandomGen.Next(MinRandomNumber, MaxRandomNumber);
    }

    private IEnumerable<string> GenerateStrings(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return GenerateString(StringLength);
        }
    }

    private static string GenerateString(int length)
    {
        Guid guid = Guid.NewGuid();
        ReadOnlySpan<byte> guidSpan = new(guid.ToByteArray());
        return Convert.ToBase64String(guidSpan).Substring(0, length);
    }

    private IEnumerable<T> ExtendCollection<T>(IEnumerable<T> collection, int targetSize)
    {
        List<T> list = collection.ToList();

        for (int i = 0; i < targetSize; i++)
        {
            yield return list[RandomGen.Next(list.Count)];
        }
    }
}

public struct RandomSong
{
    public string Artist { get; set; }
    public string Song { get; set; }
    public DateTimeOffset Time { get; set; }
}

public static class EnumerableExtensions
{
    private static readonly Random random = new();

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => random.Next());
    }
}