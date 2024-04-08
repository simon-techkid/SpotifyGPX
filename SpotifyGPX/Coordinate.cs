// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

/// <summary>
/// A latitude/longitude pair.
/// </summary>
public readonly struct Coordinate
{
    /// <summary>
    /// Creates a coordinate object with a latitude and longitude.
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lon">Longitude</param>
    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// This coordinate pair's latitude value.
    /// </summary>
    public readonly double Latitude { get; }

    /// <summary>
    /// This coordinate pair's longitude value.
    /// </summary>
    public readonly double Longitude { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not Coordinate)
        {
            return false;
        }

        Coordinate other = (Coordinate)obj;
        return Latitude == other.Latitude && Longitude == other.Longitude;
    }

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public static bool operator ==(Coordinate c1, Coordinate c2) => c1.Equals(c2);

    public static bool operator !=(Coordinate c1, Coordinate c2) => !c1.Equals(c2);

    /// <summary>
    /// Adds two coordinates together.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A coordinate representing the added coordinates.</returns>
    public static Coordinate operator +(Coordinate c1, Coordinate c2)
    {
        double latSum = c1.Latitude + c2.Latitude;
        double lonSum = c1.Longitude + c2.Longitude;
        return new Coordinate(latSum, lonSum);
    }

    /// <summary>
    /// Subtracts one coordinate from another.
    /// </summary>
    /// <param name="c1">The first coordinate</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A coordinate representing the difference between the coordinates.</returns>
    public static Coordinate operator -(Coordinate c1, Coordinate c2)
    {
        double latDiff = c1.Latitude - c2.Latitude;
        double lonDiff = c1.Longitude - c2.Longitude;
        return new Coordinate(latDiff, lonDiff);
    }

    /// <summary>
    /// Multiplies a coordinate by a scalar.
    /// </summary>
    /// <param name="c">A coordinate object.</param>
    /// <param name="scalar">The scalar value to shift the coordinate by.</param>
    /// <returns>A new coordinate representing the scaled original coordinate.</returns>
    public static Coordinate operator *(Coordinate c, double scalar)
    {
        double latScaled = c.Latitude * scalar;
        double lonScaled = c.Longitude * scalar;
        return new Coordinate(latScaled, lonScaled);
    }

    /// <summary>
    /// Calculates the distance between two coordinates.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A double representing the distance between the two coordinates.</returns>
    public static double CalculateDistance(Coordinate c1, Coordinate c2)
    {
        double latDiff = c2.Latitude - c1.Latitude;
        double lonDiff = c2.Longitude - c1.Longitude;

        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        return distance;
    }
}
