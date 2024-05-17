// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public sealed partial class PointTest : RandomPointBase
{
    protected override ParsePointsDelegate ParsePointsMethod => ParsePoints;
    protected override ParseTracksDelegate ParseTracksMethod => PointsToTracks;
    protected override DateOnly GeneratorStartDate => DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(1));
    protected override DateOnly GeneratorEndDate => DateOnly.FromDateTime(DateTime.Now);
    protected override TimeOnly DayStartTime => new(DriveStartHour, 0);
    protected override TimeOnly DayEndTime => new(DriveEndHour, 0);
    protected override TimeSpan TimeZone => new(2, 0, 0);
    protected override int PointPlacementIntervalSeconds => 15; // simulate 15 second GPS point interval

    public PointTest() : base()
    {
    }

    private List<IGpsPoint> ParsePoints()
    {
        List<RandomPoint> random = ZipAll();

        return random.Select((random, index) =>
        {
            return (IGpsPoint)new GenericPoint
            {
                Index = index,
                Location = random.Location,
                Time = random.Time
            };
        }).ToList();
    }

    private List<GpsTrack> PointsToTracks()
    {
        return ZipAll().GroupBy(dt => dt.Time.Date).Select((day, index) =>
        {
            string tn = day.Key.ToString("yyyyMMdd");
            return new GpsTrack(index, tn, TrackType.Gps, day.Select(point => (IGpsPoint)new GenericPoint { Location = point.Location, Time = point.Time }).ToList());
        }).ToList();
    }
}
