// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;

namespace SpotifyGPX.Input;

public abstract class RandomInputBase<T> : DisposableBase
{
    protected RandomInputBase(StringBroadcaster bcast) : base(bcast)
    {
        RandomGen = RandomSeed.HasValue ? new Random(RandomSeed.Value) : new Random();
    }

    /// <summary>
    /// The name of the randomizer.
    /// </summary>
    protected abstract string RandomizerName { get; }

    protected override string BroadcasterPrefix => $"RAND, {RandomizerName}";

    /// <summary>
    /// The random number generator for the <typeparamref name="T"/> generator.
    /// </summary>
    protected Random RandomGen { get; }

    /// <summary>
    /// The seed value for the random number generator, <see cref="RandomGen"/>.
    /// </summary>
    protected virtual int? RandomSeed => new Random().Next();

    /// <summary>
    /// The start date of the <typeparamref name="T"/> generator.
    /// This value represents the first date of the <typeparamref name="T"/> generator's range.
    /// </summary>
    protected abstract DateOnly GeneratorStartDate { get; }

    /// <summary>
    /// The start time of the <typeparamref name="T"/> generator.
    /// This value represents the first time of the <typeparamref name="T"/> generator's range.
    /// </summary>
    protected abstract TimeOnly DayStartTime { get; }

    /// <summary>
    /// The end date of the <typeparamref name="T"/> generator.
    /// This value represents the last date of the <typeparamref name="T"/> generator's range.
    /// </summary>
    protected abstract DateOnly GeneratorEndDate { get; }

    /// <summary>
    /// The end time of the <typeparamref name="T"/> generator.
    /// This value represents the last time of the <typeparamref name="T"/> generator's range.
    /// </summary>
    protected abstract TimeOnly DayEndTime { get; }

    /// <summary>
    /// Ensure that the time of day is within the range of <see cref="DayStartTime"/> and <see cref="DayEndTime"/>.
    /// </summary>
    protected Func<DateTimeOffset, bool> TimeCheck => dt => dt.TimeOfDay >= TimeSpan.FromHours(DayStartTime.Hour) && dt.TimeOfDay <= TimeSpan.FromHours(DayEndTime.Hour);

    /// <summary>
    /// The first <see cref="DateTimeOffset"/> of the <typeparamref name="T"/> generator.
    /// This value represents the combined values of <see cref="GeneratorStartDate"/>,
    /// <see cref="DayStartTime"/>,
    /// and uses the time zone from <see cref="TimeZone"/>.
    /// </summary>
    protected DateTimeOffset First => new(GeneratorStartDate.ToDateTime(DayStartTime), TimeZone);

    /// <summary>
    /// The last <see cref="DateTimeOffset"/> of the <typeparamref name="T"/> generator.
    /// This value represents the combined values of <see cref="GeneratorEndDate"/>,
    /// <see cref="DayEndTime"/>,
    /// and uses the time zone from <see cref="TimeZone"/>.
    /// </summary>
    protected DateTimeOffset Last => new(GeneratorEndDate.ToDateTime(DayEndTime), TimeZone);

    /// <summary>
    /// The time zone representing the zone of all <typeparamref name="T"/> objects' time values.
    /// </summary>
    protected abstract TimeSpan TimeZone { get; }

    /// <summary>
    /// Creates a list of type <typeparamref name="T"/> with a random count of elements.
    /// </summary>
    /// <returns>A list of type <typeparamref name="T"/> containing a random set of elements.</returns>
    protected abstract List<T> ZipAll();

    /// <summary>
    /// All random objects (Songs, Points, and Pairs) require <see cref="DateTimeOffset"/> time values.
    /// This method must be implemented by the derived class.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="DateTimeOffset"/>.</returns>
    protected abstract IEnumerable<DateTimeOffset> GenerateDateTimeOffsets();

    protected override void DisposeClass()
    {
    }

    protected bool IsValidTimes()
    {
        return DayStartTime < DayEndTime;
    }
}
