// SpotifyGPX by Simon Field

using System;

namespace SpotifyGPX;

/// <summary>
/// A latitude/longitude pair.
/// </summary>
public readonly struct Coordinate :
    ICloneable,
    IEquatable<Coordinate>,
    IComparable,
    IComparable<Coordinate>,
    IFormattable
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

        return Equals((Coordinate)obj);
    }

    public bool Equals(Coordinate other)
    {
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

    public object Clone()
    {
        return new Coordinate(Latitude, Longitude);
    }

    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;

        if (obj is Coordinate other)
            return CompareTo(other);

        throw new ArgumentException("Object is not a Coordinate");
    }

    public int CompareTo(Coordinate other)
    {
        var latComparison = Latitude.CompareTo(other.Latitude);
        if (latComparison != 0) return latComparison;
        return Longitude.CompareTo(other.Longitude);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"Latitude: {Latitude.ToString(format, formatProvider)}, Longitude: {Longitude.ToString(format, formatProvider)}";
    }

    public double CalculateDistance(Coordinate other)
    {
        const double R = 6371; // Radius of the Earth in km
        double lat1Rad = ToRadians(Latitude);
        double lat2Rad = ToRadians(other.Latitude);
        double deltaLat = ToRadians(other.Latitude - Latitude);
        double deltaLon = ToRadians(other.Longitude - Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in km
    }

    public static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}
