// SpotifyGPX by Simon Field

namespace SpotifyGPX;

/// <summary>
/// The StringBuilder used to add non-null objects as lines to a pair's description.
/// </summary>
public class StringBuilder
{
    private readonly System.Text.StringBuilder builder;

    public StringBuilder() => builder = new System.Text.StringBuilder();

    /// <summary>
    /// Appends the provided value to a string.
    /// </summary>
    /// <param name="format">The format of the given string.</param>
    /// <param name="value">The value to be placed on the new string.</param>
    /// <returns>The given StringBuilder, with the new string added (if the provided value wasn't null).</returns>
    public StringBuilder Append(string format, object? value)
    {
        if (value != null)
        { // If appended value not null, append the line to the builder
            builder.Append(string.Format(format, value));
        } // If null, the builder will be returned unchanged
        return this; // Return the builder
    }

    /// <summary>
    /// Appends the provided value to a string on a new line.
    /// </summary>
    /// <param name="format">The format of the given new line.</param>
    /// <param name="value">The value to be placed on the new line.</param>
    /// <returns>The given StringBuilder, with the new line added (if the provided value wasn't null).</returns>
    public StringBuilder AppendLine(string format, object? value)
    {
        if (value != null)
        { // If appended value not null, append the line to the builder
            builder.AppendLine(string.Format(format, value));
        } // If null, the builder will be returned unchanged
        return this; // Return the builder
    }

    /// <summary>
    /// Converts this StringBuilder to a string.
    /// </summary>
    /// <returns>This StringBuilder, as a string.</returns>
    public override string ToString() => builder.ToString();
}
