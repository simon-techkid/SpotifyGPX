// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public abstract class RandomPointBase : RandomInputBase<RandomPoint>, IGpsInput
{
    protected RandomPointBase() : base()
    {
    }

    public List<GpsTrack> GetAllTracks()
    {
        return AllTracks;
    }

    public List<GpsTrack> GetSelectedTracks()
    {
        return AllTracks;
    }

    public List<GpsTrack> GetFilteredTracks()
    {
        return AllTracks;
    }


    public int SourceTrackCount => AllPoints.GroupBy(point => point.Time.Date).Count();
    public int SourcePointCount => 0;
    public int ParsedTrackCount => AllTracks.Count;
    public int ParsedPointCount => AllPoints.Count;

    protected delegate List<IGpsPoint> ParsePointsDelegate();
    protected abstract ParsePointsDelegate ParsePointsMethod { get; }

    protected delegate List<GpsTrack> ParseTracksDelegate();
    protected abstract ParseTracksDelegate ParseTracksMethod { get; }
    protected List<GpsTrack> AllTracks => ParseTracksMethod();
    protected List<IGpsPoint> AllPoints => ParsePointsMethod();
    protected virtual double CenterLatitude => 44.918516; // Libourne, France
    protected virtual double CenterLongitude => -0.245090; // Libourne, France

    protected override List<RandomPoint> ZipAll()
    {
        IEnumerable<DateTimeOffset> timestamps = GenerateDateTimeOffsets();
        Coordinate[] coordinates = GenerateRandomCoordinates(10, timestamps.Count());

        var zippedData = timestamps
            .Zip(coordinates, (time, location) => new { Time = time, Location = location });

        return zippedData.Select(data => new RandomPoint { Time = data.Time, Location = data.Location }).ToList();
    }

    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// The interval in seconds between each randomly generated point.
    /// </summary>
    protected abstract int PointPlacementIntervalSeconds { get; }

    protected override IEnumerable<DateTimeOffset> GenerateDateTimeOffsets()
    {
        if (IsValidTimes() == false)
            throw new ArgumentException("Invalid start and end times");

        DateTimeOffset current = First;

        while (current < Last)
        {
            yield return current;

            TimeSpan interval = TimeSpan.FromSeconds(PointPlacementIntervalSeconds);

            current = current.Add(interval);
        }
    }

    protected Coordinate[] GenerateRandomCoordinates(double radius, int count)
    {
        Coordinate[] coordinates = new Coordinate[count];

        for (int i = 0; i < count; i++)
        {
            double angle = RandomGen.NextDouble() * 2 * Math.PI;
            double distance = Math.Sqrt(RandomGen.NextDouble()) * radius;

            double latitudeOffset = distance / EarthRadiusKm * (180 / Math.PI);
            double longitudeOffset = distance / (EarthRadiusKm * Math.Cos(Math.PI * CenterLatitude / 180)) * (180 / Math.PI);

            double newLatitude = CenterLatitude + latitudeOffset * (RandomGen.Next(2) == 1 ? 1 : -1);
            double newLongitude = CenterLongitude + longitudeOffset * (RandomGen.Next(2) == 1 ? 1 : -1);

            coordinates[i] = new Coordinate(newLatitude, newLongitude);
        }

        return coordinates;
    }

    protected static bool IsWithinBounds(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }

    protected static bool IsWithinBounds(Coordinate coordinate)
    {
        return IsWithinBounds(coordinate.Latitude, coordinate.Longitude);
    }
}

public struct RandomPoint
{
    public Coordinate Location { get; set; }
    public DateTimeOffset Time { get; set; }
}