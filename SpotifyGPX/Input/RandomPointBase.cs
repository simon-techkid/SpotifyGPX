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

    // Skip track selection for random tracks
    /*
    public List<GpsTrack> GetSelectedTracks()
    {
        return AllTracks;
    }
    */

    public abstract List<GpsTrack> ParseTracksMethod();
    public abstract List<GpsTrack> FilterTracksMethod();
    protected abstract List<IGpsPoint> ParsePointsMethod();
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

        IEnumerable<Coordinate> coordinates =
            GenerateRandomCoordinates(GenerationRadius, timestamps.Count())
            .Where(coordinate => coordinate.IsWithinBounds());

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
        // Convert radius from kilometers to degrees
        double radiusDegrees = radiusKm / 111.0;

        // Generate a random distance within the radius
        double distance = radiusDegrees * Math.Sqrt(RandomGen.NextDouble());

        // Generate a random angle
        double angle = 2 * Math.PI * RandomGen.NextDouble();

        // Calculate the offsets from the center point
        double offsetLat = distance * Math.Cos(angle);
        double offsetLon = distance * Math.Sin(angle) / Math.Cos(CenterLatitude * Math.PI / 180);

        // Calculate the new latitude and longitude
        double newLat = CenterLatitude + offsetLat;
        double newLon = CenterLongitude + offsetLon;

        return new Coordinate(newLat, newLon);
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