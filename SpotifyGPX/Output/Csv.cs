// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public sealed partial class Csv : TxtSaveable
{
    public override string FormatName => nameof(Csv).ToLower();
    protected override DocumentAccessor SaveAction => GetDocument;

    public Csv(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
    }

    private string[] GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> pairs = DataProvider();

        // Uncomment to include possibly null fields in the CSV
        //string header = string.Join(Delimiter, columns.Select(column => $"\"{column.Heading}\""));
        //string[] csvData = pairs.Select(pair => string.Join(Delimiter, columns.Select(column => $"\"{column.CellValue(pair)}\""))).ToArray();

        // Omit columns containing all null values
        IEnumerable<ColumnDefinition> nonEmptyColumns = columns.Where(column => pairs.Any(pair => column.CellValue!(pair) != null));
        string header = string.Join(Delimiter, nonEmptyColumns.Select(column => $"\"{column.Heading}\""));
        string[] csvData = pairs.Select(pair => string.Join(Delimiter, nonEmptyColumns.Select(column => $"\"{column.CellValue!(pair) ?? ""}\""))).ToArray();

        return new[] { header }.Concat(csvData).ToArray();
    }

    private class ColumnDefinition
    {
        public string? Heading { get; set; }
        public Func<SongPoint, object?>? CellValue { get; set; }
    }

    public override int Count => Document.Length - 1; // Subtract one for the header line
}
