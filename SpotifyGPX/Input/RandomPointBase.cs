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

    /* Skip track selection for random tracks
    public List<GpsTrack> GetSelectedTracks()
    {
        return AllTracks;
    }
    */

    private const double EarthRadiusKm = 6371.0;

    protected delegate List<IGpsPoint> ParsePointsDelegate();
    protected abstract ParsePointsDelegate ParsePointsMethod { get; }
    public abstract IGpsInput.ParseTracksDelegate ParseTracksMethod { get; }
    public abstract IGpsInput.FilterTracksDelegate FilterTracksMethod { get; }
    protected List<IGpsPoint> AllPoints => ParsePointsMethod();
    protected List<GpsTrack> AllTracks => ParseTracksMethod();

    /// <summary>
    /// The latitude of the center of the generated points.
    /// </summary>
    protected abstract double CenterLatitude { get; }

    /// <summary>
    /// The longitude of the center of the generated points.
    /// </summary>
    protected abstract double CenterLongitude { get; }

    /// <summary>
    /// The radius, in kilometers, from <see cref="CenterLatitude"/> and <see cref="CenterLongitude"/>, in point generation.
    /// </summary>
    protected abstract double GenerationRadius { get; }

    /// <summary>
    /// The interval in seconds between each randomly generated point.
    /// </summary>
    protected abstract int PointPlacementIntervalSeconds { get; }

    

    protected override List<RandomPoint> ZipAll()
    {
        IEnumerable<DateTimeOffset> timestamps = 
            GenerateDateTimeOffsets()
            .Where(TimeCheck);

        IEnumerable<Coordinate> coordinates = GenerateRandomCoordinates(GenerationRadius, timestamps.Count());

        var zippedData = timestamps
            .Zip(coordinates, (time, location) => new { Time = time, Location = location });

        return zippedData.Select(data => new RandomPoint { Time = data.Time, Location = data.Location }).ToList();
    }

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

    private IEnumerable<Coordinate> GenerateRandomCoordinates(double radiusKm, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return GenerateRandomCoordinate(radiusKm);
        }
    }

    private Coordinate GenerateRandomCoordinate(double radiusKm)
    {
        double distance = radiusKm * Math.Sqrt(RandomGen.NextDouble());
        double angle = RandomGen.NextDouble() * 2 * Math.PI;
        double distanceRadians = distance / EarthRadiusKm;

        double centerLatRadians = Coordinate.ToRadians(CenterLatitude);
        double centerLonRadians = Coordinate.ToRadians(CenterLongitude);

        double newLat = Math.Asin(
            Math.Sin(centerLatRadians) *
            Math.Cos(distanceRadians) +
            Math.Cos(centerLatRadians) *
            Math.Sin(distanceRadians) *
            Math.Cos(angle));

        double newLon = centerLonRadians +
                        Math.Atan2(Math.Sin(angle) *
                        Math.Sin(distanceRadians) *
                        Math.Cos(centerLatRadians),
                                   Math.Cos(distanceRadians) -
                                   Math.Sin(centerLatRadians) *
                                   Math.Sin(newLat));

        return new Coordinate(Coordinate.ToDegrees(newLat), Coordinate.ToDegrees(newLon));
    }

    public int SourceTrackCount => AllPoints.GroupBy(point => point.Time.Date).Count();
    public int SourcePointCount => 0;
    public int ParsedTrackCount => AllTracks.Count;
    public int ParsedPointCount => AllPoints.Count;
}

public struct RandomPoint
{
    public Coordinate Location { get; set; }
    public DateTimeOffset Time { get; set; }
}