// SpotifyGPX by Simon Field

using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpotifyGPX.Output;

public partial class Xlsx : ByteSaveable
{
    public override string FormatName => nameof(Xlsx).ToLower();
    private ExcelPackage Package { get; set; }

    public Xlsx(Func<IEnumerable<SongPoint>> pairs, string? trackName) : base(pairs, trackName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        Package = new();
        Count = GetPackage();
    }

    protected override byte[] GetDocument(string? trackName)
    {
        return Package.GetAsByteArray();
    }

    private readonly List<ColumnDefinition> columns = new()
        {
            new ColumnDefinition
            {
                Heading = "Artist",
                CellValue = pair => pair.Song.Song_Artist,
                TotalSettings = new()
                {
                    TotalFormula = StringMode,
                }
            },
            new ColumnDefinition
            {
                Heading = "Album",
                CellValue = pair => pair.Song.GetPropertyValue<IAlbumableSong>(song => song.Song_Album),
                TotalSettings = new()
                {
                    TotalFormula = StringMode,
                }
            },
            new ColumnDefinition
            {
                Heading = "Title",
                CellValue = pair => pair.Song.Song_Name,
                TotalSettings = new()
                {
                    TotalFormula = StringMode,
                },
                DupeVals = new()
            },
            new ColumnDefinition
            {
                Heading = "Duration",
                CellValue = pair => pair.Song.GetPropertyValue<IDuratableSong>(song => song.TimePlayed),
                NumberFormat = TimeSpan,
                TotalSettings = new()
                {
                    TotalFormula = Sum,
                    TotalFormat = TimeSpan
                },
                DataBar = new()
            },
            new ColumnDefinition
            {
                Heading = "Song Time",
                CellValue = pair => pair.SongTime.UtcDateTime,
                NumberFormat = TimeFormat,
                TotalSettings = new()
                {
                    TotalFormula = LastToFirstDiff,
                    TotalFormat = TimeSpan
                }
            },
            new ColumnDefinition
            {
                Heading = "Latitude",
                CellValue = pair => pair.Point.Location.Latitude,
                NumberFormat = Double,
                DupeVals = new()
            },
            new ColumnDefinition
            {
                Heading = "Longitude",
                CellValue = pair => pair.Point.Location.Longitude,
                NumberFormat = Double,
                DupeVals = new()
            },
            new ColumnDefinition
            {
                Heading = "Point Time",
                CellValue = pair => pair.PointTime.UtcDateTime,
                NumberFormat = TimeFormat,
                TotalSettings = new()
                {
                    TotalFormula = LastToFirstDiff,
                    TotalFormat = TimeSpan
                }
            },
            new ColumnDefinition
            {
                Heading = "Accuracy",
                CellValue = pair => pair.Accuracy,
                NumberFormat = Double,
                DataBar = new()
            },
            new ColumnDefinition
            {
                Heading = "AbsAccuracy",
                CellValue = pair => pair.AbsAccuracy,
                NumberFormat = Double,
                TotalSettings = new()
                {
                    TotalFormula = Avg,
                    TotalFormat = Double
                },
                DataBar = new()
            }
        };

    private int GetPackage()
    {
        IEnumerable<IGrouping<TrackInfo, SongPoint>> tracks = GroupedDataProvider(pair => pair.Origin);

        int totalPairs = 0;

        foreach (IGrouping<TrackInfo, SongPoint> track in tracks)
        {
            string trackGuid = Guid.NewGuid().ToString().AsSpan(0, 8).ToString();
            string trackName = track.Key.ToString();

            FilledCells cells = GetWorksheetForTrack(trackName, track);
            ExcelWorksheet worksheet = Package.Workbook.Worksheets.Add(trackName, cells.Worksheet);
            totalPairs += cells.Count;
            worksheet.TabColor = GetColorForWorksheet();

            string tableName = $"Table_{trackGuid}";
            ExcelTable table = worksheet.Tables.Add(cells.Range, tableName);
            table.TableStyle = TableStyle;
            table.ShowTotal = CreateTotals;

            foreach (ColumnDefinition column in columns)
            {
                int columnIndex = StartColumn + columns.IndexOf(column) + 1;
                string columnName = ExcelCellAddress.GetColumnLetter(columnIndex);

                int firstRow = cells.Range.Start.Row + 1; // Add one to skip table header
                int lastRow = cells.Range.End.Row;

                if (ConditionalFormatting)
                    AddConditionalFormatting(columnIndex, columnName, worksheet, column, firstRow, lastRow);

                if (CreateTotals && column.TotalSettings != null)
                    AddTotalRow(columnIndex, columnName, worksheet, table, column, firstRow, lastRow);
            }
        }

        return totalPairs;
    }

    private void AddConditionalFormatting(int columnIndex, string columnName, ExcelWorksheet worksheet, ColumnDefinition column, int firstRow, int lastRow)
    {
        ExcelRange thisColumn = worksheet.Cells[$"{columnName}{firstRow}:{columnName}{lastRow}"];

        column.DataBar?.Add(thisColumn);
        column.DupeVals?.Add(thisColumn);
        column.UniqueVals?.Add(thisColumn);
    }

    private void AddTotalRow(int columnIndex, string columnName, ExcelWorksheet worksheet, ExcelTable table, ColumnDefinition column, int firstRow, int lastRow)
    {
        if (column.TotalSettings?.TotalFormula != null)
        {
            string totalFormula = string.Format(column.TotalSettings.TotalFormula, columnName, column.Heading, firstRow, lastRow);
            table.Columns[columnIndex - 1].TotalsRowFormula = totalFormula;
        }

        if (column.TotalSettings?.TotalFormat != null)
        {
            worksheet.Cells[$"{columnName}{lastRow + 1}"].Style.Numberformat.Format = column.TotalSettings.TotalFormat;
        }
    }

    private FilledCells GetWorksheetForTrack(string trackName, IEnumerable<SongPoint> track)
    {
        ExcelPackage package = new();
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(trackName);

        int minRow = StartRow;
        int maxRow = 0;
        int minCol = StartColumn + 1;
        int maxCol = 0;

        List<SongPoint> allPairs = track.ToList();

        for (int i = 0; i < columns.Count; i++)
        {
            ColumnDefinition column = columns[i];

            int rowIndex = minRow; // Return to top row
            int columnIndex = minCol + i; // Go to the right column

            List<object?> rows = column.GetValues(allPairs);

            worksheet.Cells[rowIndex++, columnIndex].Value = column.Heading;

            foreach (var row in rows)
            {
                worksheet.Cells[rowIndex, columnIndex].Value = row ?? string.Empty;
                worksheet.Cells[rowIndex, columnIndex].Style.Numberformat.Format = column.NumberFormat;
                rowIndex++;
            }

            maxRow = Math.Max(maxRow, rowIndex - 1); // minus header row
            maxCol = Math.Max(maxCol, columnIndex);
        }

        ExcelRange filledCells = worksheet.Cells[minRow, minCol, maxRow, maxCol];

        int pairsInSheet = maxRow - minRow;

        return new FilledCells(filledCells, pairsInSheet, worksheet);
    }

    private readonly struct FilledCells
    {
        public FilledCells(ExcelRange range, int count, ExcelWorksheet worksheet)
        {
            Range = range;
            Count = count;
            Worksheet = worksheet;
        }

        public ExcelRange Range { get; }
        public int Count { get; }
        public ExcelWorksheet Worksheet { get; }
    }

    private static Color GetColorForWorksheet()
    {
        Random random = new();
        return ColorSheets ? Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)) : Color.White;
    }

    private class ConditionalFormattingDuplicateValues
    {
        private Color? _backgroundColor;
        private Color? _foregroundColor;
        private ExcelFillStyle? _fillStyle;

        /// <summary>
        /// Add a duplicate values conditional formatting to the specified range.
        /// </summary>
        /// <param name="here">An ExcelRange where the conditional formatting will be applied.</param>
        public void Add(ExcelRange here)
        {
            var dupeVals = here.ConditionalFormatting.AddDuplicateValues();
            dupeVals.Style.Fill.PatternType = FillStyle;
            dupeVals.Style.Fill.BackgroundColor.Color = BackgroundColor;
            dupeVals.Style.Font.Color.Color = ForegroundColor;
        }

        /// <summary>
        /// The background color of the duplicate values conditional formatting. Default: light red.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                if (_backgroundColor != null)
                    return (Color)_backgroundColor;
                else
                    return Color.FromArgb(255, 199, 206);
            }
            set
            {
                _backgroundColor = value;
            }
        }

        /// <summary>
        /// The font color of the duplicate values conditional formatting. Default: dark red.
        /// </summary>
        public Color ForegroundColor
        {
            get
            {
                if (_foregroundColor != null)
                    return (Color)_foregroundColor;
                else
                    return Color.FromArgb(156, 0, 6);
            }
            set
            {
                _foregroundColor = value;
            }
        }

        /// <summary>
        /// The fill style of the duplicate values conditional formatting. Default: solid.
        /// </summary>
        public ExcelFillStyle FillStyle
        {
            get
            {
                if (_fillStyle != null)
                    return (ExcelFillStyle)_fillStyle;
                else
                    return ExcelFillStyle.Solid;
            }
            set
            {
                _fillStyle = value;
            }
        }
    }

    private class ConditionalFormattingUniqueValues
    {
        private Color? _backgroundColor;
        private Color? _foregroundColor;
        private ExcelFillStyle? _fillStyle;

        /// <summary>
        /// Add a unique values conditional formatting to the specified range.
        /// </summary>
        /// <param name="here">An ExcelRange where the conditional formatting will be applied.</param>
        public void Add(ExcelRange here)
        {
            var uniqueVals = here.ConditionalFormatting.AddUniqueValues();
            uniqueVals.Style.Fill.PatternType = FillStyle;
            uniqueVals.Style.Fill.BackgroundColor.Color = BackgroundColor;
            uniqueVals.Style.Font.Color.Color = ForegroundColor;
        }

        /// <summary>
        /// The background color of the unique values conditional formatting. Default: light green.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                if (_backgroundColor != null)
                    return (Color)_backgroundColor;
                else
                    return Color.FromArgb(198, 239, 206);
            }
            set
            {
                _backgroundColor = value;
            }
        }

        /// <summary>
        /// The font color of the unique values conditional formatting. Default: dark green.
        /// </summary>
        public Color ForegroundColor
        {
            get
            {
                if (_foregroundColor != null)
                    return (Color)_foregroundColor;
                else
                    return Color.FromArgb(0, 97, 0);
            }
            set
            {
                _foregroundColor = value;
            }
        }

        /// <summary>
        /// The fill style of the unique values conditional formatting. Default: solid.
        /// </summary>
        public ExcelFillStyle FillStyle
        {
            get
            {
                if (_fillStyle != null)
                    return (ExcelFillStyle)_fillStyle;
                else
                    return ExcelFillStyle.Solid;
            }
            set
            {
                _fillStyle = value;
            }
        }
    }

    private class ConditionalFormattingDataBar
    {
        private Color? _color;
        private bool? _gradient;
        private bool? _negativeBarColorSameAsPositive;
        private eExcelConditionalFormattingValueObjectType? _lowValue;
        private eExcelConditionalFormattingValueObjectType? _highValue;
        private bool? _showValue;

        /// <summary>
        /// Add data bars to the specified range.
        /// </summary>
        /// <param name="here">An ExcelRange where the conditional formatting will be applied.</param>
        public void Add(ExcelRange here)
        {
            var dataBar = here.ConditionalFormatting.AddDatabar(Color);
            dataBar.Gradient = Gradient;
            dataBar.NegativeBarColorSameAsPositive = NegativeBarColorSameAsPositive;
            dataBar.LowValue.Type = LowValue;
            dataBar.HighValue.Type = HighValue;
            dataBar.ShowValue = ShowValue;
        }

        /// <summary>
        /// The background color of the data bar. Default: light blue.
        /// </summary>
        public Color Color
        {
            get
            {
                if (_color != null)
                    return (Color)_color;
                else
                    return Color.FromArgb(0, 176, 240);
            }
            set
            {
                _color = value;
            }
        }

        /// <summary>
        /// Whether or not the data bar should have its color fill as a gradient. Default: false.
        /// </summary>
        public bool Gradient
        {
            get
            {
                if (_gradient != null)
                    return (bool)_gradient;
                else
                    return false;
            }
            set
            {
                _gradient = value;
            }
        }

        /// <summary>
        /// Whether or not negative values should share the same color data bar. Default: false.
        /// </summary>
        public bool NegativeBarColorSameAsPositive
        {
            get
            {
                if (_negativeBarColorSameAsPositive != null)
                    return (bool)_negativeBarColorSameAsPositive;
                else
                    return false;
            }
            set
            {
                _negativeBarColorSameAsPositive = value;
            }
        }

        /// <summary>
        /// The method to use for the low value of the data bar. Default: auto min.
        /// </summary>
        public eExcelConditionalFormattingValueObjectType LowValue
        {
            get
            {
                if (_lowValue != null)
                    return (eExcelConditionalFormattingValueObjectType)_lowValue;
                else
                    return eExcelConditionalFormattingValueObjectType.AutoMin;
            }
            set
            {
                _lowValue = value;
            }
        }

        /// <summary>
        /// The method to use for the high value of the data bar. Default: auto max.
        /// </summary>
        public eExcelConditionalFormattingValueObjectType HighValue
        {
            get
            {
                if (_highValue != null)
                    return (eExcelConditionalFormattingValueObjectType)_highValue;
                else
                    return eExcelConditionalFormattingValueObjectType.AutoMax;
            }
            set
            {
                _highValue = value;
            }
        }

        /// <summary>
        /// Whether or not to show the value of the cells along with the data bar. Default: true.
        /// </summary>
        public bool ShowValue
        {
            get
            {
                if (_showValue != null)
                    return (bool)_showValue;
                else
                    return true;
            }
            set
            {
                _showValue = value;
            }
        }
    }

    /// <summary>
    /// An Excel table column
    /// </summary>
    private class ColumnDefinition
    {
        /// <summary>
        /// The name of this column
        /// </summary>
        public string? Heading { get; set; }

        /// <summary>
        /// The SongPoint value to be placed within this column's cells
        /// </summary>
        public Func<SongPoint, object?>? CellValue { get; set; }

        /// <summary>
        /// Get the value for a SongPoint given the column's CellValue function.
        /// </summary>
        /// <param name="here">A SongPoint object.</param>
        /// <returns>An object representing the value of this SongPoint.</returns>
        public object? GetValue(SongPoint here)
        {
            return CellValue != null ? CellValue(here) : null;
        }

        public List<object?> GetValues(IEnumerable<SongPoint> points)
        {
            return points.Select(GetValue).ToList();
        }

        /// <summary>
        /// The number format to use for all cells of this column
        /// </summary>
        public string? NumberFormat { get; set; }

        /// <summary>
        /// The settings for this column's grand total calculation
        /// </summary>
        public TotalRowSettings? TotalSettings { get; set; }

        /// <summary>
        /// If not null, apply data bars to this column.
        /// </summary>
        public ConditionalFormattingDataBar? DataBar { get; set; }

        /// <summary>
        /// If not null, apply duplicate values highlighting to this column.
        /// </summary>
        public ConditionalFormattingDuplicateValues? DupeVals { get; set; }

        /// <summary>
        /// If not null, apply unique values highlighting to this column.
        /// </summary>
        public ConditionalFormattingUniqueValues? UniqueVals { get; set; }
    }

    private class TotalRowSettings
    {
        private string? _totalFormula;
        private string? _totalFormat;

        /// <summary>
        /// The formula to invoke in the Grand Total cell for this column
        /// </summary>
        public string TotalFormula
        {
            get
            {
                if (_totalFormula != null)
                    return _totalFormula;
                else
                    return string.Empty;
            }
            set
            {
                _totalFormula = value;
            }
        }

        /// <summary>
        /// The number format to use for the Grand Total cell of this column
        /// </summary>
        public string TotalFormat
        {
            get
            {
                if (_totalFormat != null)
                    return _totalFormat;
                else
                    return string.Empty;
            }
            set
            {
                _totalFormat = value;
            }
        }
    }

    public override int Count { get; }
}
