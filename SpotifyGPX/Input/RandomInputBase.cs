// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;

namespace SpotifyGPX.Input;

public abstract class RandomInputBase<T> : IDisposable
{
    protected Random RandomGen { get; }
    protected virtual int? RandomSeed => new Random().Next();

    protected virtual int MinTimesPerDay => 100;
    protected virtual int MinRandomNumber => 0;
    protected virtual int MaxRandomNumber => 100;

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

    protected RandomInputBase()
    {
        RandomGen = RandomSeed.HasValue ? new Random(RandomSeed.Value) : new Random();
    }

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

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        return;
    }

    protected bool IsValidTimes()
    {
        return DayStartTime < DayEndTime;
    }
}
