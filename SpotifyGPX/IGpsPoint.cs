// SpotifyGPX by Simon Field

using SpotifyGPX.Api;
using SpotifyGPX.Api.Geocoding;
using System;

namespace SpotifyGPX;

/// <summary>
/// Interfaces with structs designated for GPS point records.
/// All structs encapsulating GPS point records must implement this interface.
/// </summary>
public interface IGpsPoint :
    IInterfaceFront<IGpsPoint>,
    IApiMetadataRecordable<Coordinate, IGpsMetadata>
{
    /// <summary>
    /// The description of this point, as printed to description fields.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Unique identifier of this GPS point in a list.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// The coordinate (latitude/longitude pair) of this point on Earth.
    /// </summary>
    public Coordinate Location { get; set; }

    /// <summary>
    /// The time and date at which this point was recorded.
    /// </summary>
    public DateTimeOffset Time { get; set; }
}
