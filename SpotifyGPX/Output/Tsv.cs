// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public sealed partial class Tsv : TxtSaveable
{
    public override string FormatName => "tsv";

    public Tsv(Func<IEnumerable<SongPoint>> pairs, string? trackName, Broadcaster bcast) : base(pairs, trackName, bcast)
    {
    }

    protected override string?[] GetDocument(string? trackName)
    {
        IEnumerable<SongPoint> pairs = DataProvider();

        // Uncomment to include possibly null fields in the TSV
        //string header = string.Join(Delimiter, columns.Select(column => $"\"{column.Heading}\""));
        //string[] tsvData = pairs.Select(pair => string.Join(Delimiter, columns.Select(column => $"\"{column.CellValue(pair)}\""))).ToArray();

        // Omit columns containing all null values
        IEnumerable<ColumnDefinition> nonEmptyColumns = columns.Where(column => pairs.Any(pair => column.CellValue!(pair) != null));
        string header = string.Join(Delimiter, nonEmptyColumns.Select(column => $"\"{column.Heading}\""));
        string[] tsvData = pairs.Select(pair => string.Join(Delimiter, nonEmptyColumns.Select(column => $"\"{column.CellValue!(pair) ?? ""}\""))).ToArray();

        return new[] { header }.Concat(tsvData).ToArray();
    }

    private class ColumnDefinition
    {
        public string? Heading { get; set; }
        public Func<SongPoint, object?>? CellValue { get; set; }
    }

    public override int Count => Document.Length - 1;
}
