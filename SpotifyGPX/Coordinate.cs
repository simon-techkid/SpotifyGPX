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
    IFormattable,
    IParsable<Coordinate>
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
    /// Calculates the distance between two coordinates, assuming the Earth is flat.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A <see langword="double"/> representing the flat distance between the two coordinates.</returns>
    public static double operator %(Coordinate c1, Coordinate c2)
    {
        double latDiff = c2.Latitude - c1.Latitude;
        double lonDiff = c2.Longitude - c1.Longitude;

        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        return distance;
    }

    /// <summary>
    /// Calculates the distance between two coordinates using the Haversine formula.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A <see langword="double"/> representing the distance (in kilometers) between the two coordinates.</returns>
    public static double operator ^(Coordinate c1, Coordinate c2)
    {
        const double R = 6371; // Radius of the Earth in km
        double lat1Rad = ToRadians(c1.Latitude);
        double lat2Rad = ToRadians(c2.Latitude);
        double deltaLat = ToRadians(c2.Latitude - c1.Latitude);
        double deltaLon = ToRadians(c2.Longitude - c1.Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in km
    }

    /// <summary>
    /// Calculates the bearing between two coordinates.
    /// </summary>
    /// <param name="c1">The first coordinate.</param>
    /// <param name="c2">The second coordinate.</param>
    /// <returns>A <see langword="double"/> representing the bearing (in degrees) between the first and the second coordinates.</returns>
    public static double operator /(Coordinate c1, Coordinate c2)
    {
        double lat1Rad = ToRadians(c1.Latitude);
        double lat2Rad = ToRadians(c2.Latitude);
        double lon1Rad = ToRadians(c1.Longitude);
        double lon2Rad = ToRadians(c2.Longitude);

        double y = Math.Sin(lon2Rad - lon1Rad) * Math.Cos(lat2Rad);
        double x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                   Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(lon2Rad - lon1Rad);

        double bearingRad = Math.Atan2(y, x);
        double bearingDeg = (bearingRad * 180.0 / Math.PI + 360.0) % 360.0; // Convert radians to degrees and normalize

        return bearingDeg;
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
        return (Latitude + Longitude).CompareTo(other.Latitude + other.Longitude);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"Latitude: {Latitude.ToString(format, formatProvider)}, Longitude: {Longitude.ToString(format, formatProvider)}";
    }

    /// <summary>
    /// Parses a string representation of a coordinate in the format "latitude,longitude".
    /// </summary>
    /// <param name="input">The string representation of the coordinate.</param>
    /// <returns>A <see cref="Coordinate"/> object parsed from the input string.</returns>
    public static Coordinate Parse(string input, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input string cannot be null or empty.");
        }

        string[] parts = input.Split(',');

        if (parts.Length != 2)
        {
            throw new FormatException("Input string must be in the format 'latitude,longitude'.");
        }

        if (!double.TryParse(parts[0], out double latitude) || !double.TryParse(parts[1], provider, out double longitude))
        {
            throw new FormatException("Latitude and longitude must be valid double values.");
        }

        return new Coordinate(latitude, longitude);
    }

    /// <summary>
    /// Tries to parse a string representation of a coordinate in the format "latitude,longitude".
    /// </summary>
    /// <param name="input">The string representation of the coordinate.</param>
    /// <param name="result">When this method returns, contains the parsed Coordinate object, if the parsing succeeds.</param>
    /// <returns><see langword="true"/> if the parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? input, IFormatProvider? provider, out Coordinate result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string[] parts = input.Split(',');

        if (parts.Length != 2)
        {
            return false;
        }

        if (!double.TryParse(parts[0], out double latitude) || !double.TryParse(parts[1], provider, out double longitude))
        {
            return false;
        }

        result = new Coordinate(latitude, longitude);
        return true;
    }

    public static string CalculateCompassDirection(double bearingDeg)
    {
        string[] compassDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
        int index = (int)((bearingDeg + 22.5) / 45.0);
        return compassDirections[index];
    }

    public static double ToRadians(double angle) => Math.PI * angle / 180.0;

    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    public static bool IsWithinBounds(double latitude, double longitude) =>
        latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;

    public bool IsWithinBounds() => IsWithinBounds(Latitude, Longitude);
}
