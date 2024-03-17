// SpotifyGPX by Simon Field

using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public partial class Xlsx : ByteSaveable
{
    protected override byte[] Document { get; }

    public Xlsx(IEnumerable<SongPoint> pairs)
    {
        (byte[] doc, int totalPairs) = GetDocument(pairs);

        Document = doc;
        Count = totalPairs;
    }

    private static (byte[], int) GetDocument(IEnumerable<SongPoint> pairs)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        ExcelPackage package = new();

        int totalPairs = 0;

        // Create a worksheet for each track
        pairs.GroupBy(pair => pair.Origin)
            .ToList()
            .ForEach(track =>
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(track.Key.ToString());

                // Headers
                worksheet.Cells[1, 1].Value = "#"; // A
                worksheet.Cells[1, 2].Value = "Index"; // B
                worksheet.Cells[1, 3].Value = "Latitude"; // C
                worksheet.Cells[1, 4].Value = "Longitude"; // D
                worksheet.Cells[1, 5].Value = "Artist"; // E
                worksheet.Cells[1, 6].Value = "Title"; // F
                worksheet.Cells[1, 7].Value = "Album"; // G
                worksheet.Cells[1, 8].Value = "Song Time"; // H
                worksheet.Cells[1, 9].Value = "Point Time"; // I
                worksheet.Cells[1, 10].Value = "Accuracy"; // J
                worksheet.Cells[1, 11].Value = "AbsAccuracy"; // K

                // Data
                int row = 2;
                track.ToList().ForEach(pair =>
                {
                    worksheet.Cells[row, 1].Value = row - 1; // A
                    worksheet.Cells[row, 2].Value = pair.Index; // B
                    worksheet.Cells[row, 3].Value = pair.Point.Location.Latitude; // C
                    worksheet.Cells[row, 4].Value = pair.Point.Location.Longitude; // D
                    worksheet.Cells[row, 5].Value = pair.Song.Song_Artist; // E
                    worksheet.Cells[row, 6].Value = pair.Song.Song_Name; // F
                    worksheet.Cells[row, 7].Value = pair.Song.Song_Album; // G
                    worksheet.Cells[row, 8].Value = pair.PointTime.UtcDateTime; // H
                    worksheet.Cells[row, 8].Style.Numberformat.Format = Options.ISO8601UTC; // Use UTC format for UtcDateTime DateTimeOffset
                    worksheet.Cells[row, 9].Value = pair.SongTime.UtcDateTime; // I
                    worksheet.Cells[row, 9].Style.Numberformat.Format = Options.ISO8601UTC;
                    worksheet.Cells[row, 10].Value = pair.Accuracy; // J
                    worksheet.Cells[row, 11].Formula = $"ABS(J{row})"; // K
                    row++;
                    totalPairs++;
                });

                // Add Excel table
                var range = worksheet.Cells[1, 1, row - 1, 11]; // Select the range for the table
                var tableName = string.Concat("Table_", Guid.NewGuid().ToString().AsSpan(0, 8)); // Generate a unique table name
                var table = worksheet.Tables.Add(range, tableName); // Add table with name "Table1"
                table.TableStyle = TableStyles.Light1; // Apply a table style
                table.ShowTotal = true; // Show total row for the table
                table.Columns[10].TotalsRowFunction = RowFunctions.Average; // Set the function for the "AbsAccuracy" column
            });

        return (package.GetAsByteArray(), totalPairs);
    }

    public override int Count { get; }

}
