// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

public abstract class EasterEggs<T> : StringBroadcasterBase
{
    protected EasterEggs(StringBroadcaster bcast) : base(bcast) { }

    protected override string BroadcasterPrefix => "EGG";

    /// <summary>
    /// Defines the conditions for Easter eggs of type <typeparamref name="T"/>.
    /// </summary>
    protected abstract Func<T, bool>[] EggParameters { get; }

    /// <summary>
    /// Defines how to get the egg parameter of type <typeparamref name="T"/> from SongPoint.
    /// </summary>
    protected abstract Func<SongPoint, T> GetEggParameter { get; }

    /// <summary>
    /// Checks if a given SongPoint satisfies any of the Easter egg conditions.
    /// </summary>
    /// <param name="point">A <see cref="SongPoint"/> to check.</param>
    /// <returns>True, if the given <see cref="SongPoint"/> satisfies any of the Easter egg conditions; otherwise, false.</returns>
    public bool CheckEgg(SongPoint point)
    {
        var parameter = GetEggParameter(point);
        return parameter != null && EggParameters.Any(predicate => predicate(parameter));
    }

    /// <summary>
    /// Checks all SongPoints in the list for Easter eggs and prints a message if found.
    /// </summary>
    /// <param name="points">A list of <see cref="SongPoint"/>s to check.</param>
    public void CheckAllPairsForEggs(IEnumerable<SongPoint> points)
    {
        var foundEggs = new HashSet<string>();

        foreach (var point in points)
        {
            if (CheckEgg(point))
            {
                var eggString = EggAsString(point);
                if (foundEggs.Add(eggString))
                {
                    BCaster.Broadcast($"You found an egg: {eggString}. You've got a great taste in {Taste}!");
                }
            }
        }
    }

    /// <summary>
    /// Defines the type of taste for the Easter eggs of type <typeparamref name="T"/>.
    /// </summary>
    protected abstract string Taste { get; }

    /// <summary>
    /// Converts a <see cref="SongPoint"/> to a <see langword="string"/> for display.
    /// </summary>
    /// <param name="point">A <see cref="SongPoint"/> to convert.</param>
    /// <returns>A <see langword="string"/> representation of the <see cref="SongPoint"/>.</returns>
    protected abstract string EggAsString(SongPoint point);
}

public class SongEasterEggs : EasterEggs<ISongEntry>
{
    public SongEasterEggs(StringBroadcaster bcast) : base(bcast) { }

    protected override Func<ISongEntry, bool>[] EggParameters => new Func<ISongEntry, bool>[]
    {
        song => song.Song_Name == "Hello" && song.Song_Artist == "Adele",
        song => song.Song_Name == "Never Gonna Give You Up" && song.Song_Artist == "Rick Astley",
        song => song.Song_Name == "All Star" && song.Song_Artist == "Smash Mouth",
        song => song.Song_Name == "Africa" && song.Song_Artist == "Toto",
        song => song.Song_Name == "Take On Me" && song.Song_Artist == "a-ha",
        song => song.Song_Name == "Here Again" && song.Song_Artist == "Ten Fé",
        song => song.Song_Name == "On A Midnight Street" && song.Song_Artist == "Matthew And The Atlas",
        song => song.Song_Name == "Watching the Wires" && song.Song_Artist == "Hiss Golden Messenger",
        song => song.Song_Name == "Thought You'd Never Ask" && song.Song_Artist == "Folk Road Show",
        song => song.Song_Name == "Not Every River" && song.Song_Artist == "Bear's Den",
        song => song.Song_Name == "Won't Happen - Stripped-Back" && song.Song_Artist == "Ten Fé",
        song => song.Song_Name == "Sea so Blue" && song.Song_Artist == "Austin Basham"
    };

    protected override Func<SongPoint, ISongEntry> GetEggParameter => point => point.Song;

    protected override string Taste => "music";

    protected override string EggAsString(SongPoint point)
    {
        var song = GetEggParameter(point);
        return song != null ? song.ToString() : "Unknown Song";
    }
}
