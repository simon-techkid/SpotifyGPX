// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

/// <summary>
/// The StringBuilder used to add non-null objects as lines to a pair's description.
/// </summary>
public class StringBuilder : IFormatProvider, ICustomFormatter
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
    /// Trims the trailing newlines from the builder.
    /// </summary>
    /// <returns>The StringBuilder instance with trailing newlines removed.</returns>
    private void TrimEndNewlines()
    {
        string newline = Environment.NewLine;
        int newlineLength = newline.Length;

        while (builder.Length >= newlineLength && builder.ToString(builder.Length - newlineLength, newlineLength) == newline)
        {
            builder.Length -= newlineLength;
        }
    }

    /// <summary>
    /// Converts this StringBuilder to a string.
    /// </summary>
    /// <returns>This StringBuilder, as a string.</returns>
    public override string ToString()
    {
        TrimEndNewlines();
        return builder.ToString();
    }

    public object? GetFormat(Type? formatType)
    {
        if (formatType == typeof(ICustomFormatter))
        {
            return this;
        }
        return null;
    }

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (arg == null) return string.Empty;

        if (formatProvider != this)
        {
            if (arg is IFormattable formattable)
            {
                return formattable.ToString(format, formatProvider);
            }
            return arg.ToString() ?? string.Empty;
        }

        return string.Format(format ?? string.Empty, arg);
    }
}
