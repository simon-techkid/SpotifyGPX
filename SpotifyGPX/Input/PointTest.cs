// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using SpotifyGPX.PointEntry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public sealed partial class PointTest : RandomPointBase
{
    protected override string RandomizerName => nameof(PointTest);
    protected override List<IGpsPoint> ParsePointsMethod() => ParsePoints();
    public override List<GpsTrack> ParseTracksMethod() => ParseTracks();
    public override List<GpsTrack> FilterTracksMethod() => FilterTracks();
    protected override DateOnly GeneratorStartDate => DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(DaysPriorToTodayToGenerate));
    protected override DateOnly GeneratorEndDate => DateOnly.FromDateTime(DateTime.Now);
    private int DriveStartHour => RandomGen.Next(DriveMinStartHour, DriveMaxStartHour);
    private int DriveStartMinute => RandomGen.Next(DriveMinStartMinute, DriveMaxStartMinute);
    private int DriveEndHour => RandomGen.Next(DriveMinEndHour, DriveMaxEndHour);
    private int DriveEndMinute => RandomGen.Next(DriveMinEndMinute, DriveMaxEndMinute);
    protected override TimeOnly DayStartTime => new(DriveStartHour, DriveStartMinute);
    protected override TimeOnly DayEndTime => new(DriveEndHour, DriveEndMinute);
    protected override TimeSpan TimeZone => new(2, 0, 0);
    protected override double CenterLatitude => CenterLat;
    protected override double CenterLongitude => CenterLon;
    protected override double GenerationRadius => CenterRadius;
    protected override int PointPlacementIntervalSeconds => RandomGen.Next(MinPlacementSecs, MaxPlacementSecs); // simulate 15-120 second GPS point interval

    public PointTest(StringBroadcaster bcast) : base(bcast)
    {
    }

    private List<IGpsPoint> ParsePoints()
    {
        return ZipAll()
            .Select((random, index) =>
            {
                return (IGpsPoint)new GenericPoint
                {
                    Index = index,
                    Location = random.Location,
                    Time = random.Time
                };
            }).ToList();
    }

    private List<GpsTrack> ParseTracks()
    {
        return ZipAll()
            .GroupBy(dt => dt.Time.Date)
            .Select((day, trackIndex) =>
            {
                string tn = day.Key.ToString("yyyyMMdd");

                List<IGpsPoint> dayPoints = day.Select((point, pointIndex) =>
                {
                    return (IGpsPoint)new GenericPoint
                    {
                        Index = pointIndex,
                        Location = point.Location,
                        Time = point.Time
                    };
                }).ToList();

                return new GpsTrack(trackIndex, tn, TrackType.Gps, dayPoints);

            }).ToList();
    }

    private List<GpsTrack> FilterTracks()
    {
        return AllTracks;
    }
}
