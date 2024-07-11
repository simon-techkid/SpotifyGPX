// SpotifyGPX by Simon Field

using Geocoding.Google;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Api.Geocoding.Google;

public struct GeocodedGoogleEntry : IGpsMetadata
{
    public IEnumerable<GoogleAddress> Addresses { get; set; }

    public static string String(GoogleAddress address)
    {
        StringBuilder sb = new();

        sb.AppendLine("Data Provider: {0}", address.Provider);
        sb.AppendLine("Address: {0}", address.FormattedAddress);
        sb.AppendLine("Type: {0}", address.Type);
        sb.AppendLine("Location Type: {0}", address.LocationType);
        sb.AppendLine("Place ID: {0}", address.PlaceId);

        return sb.ToString();
    }

    public readonly string Stringify()
    {
        return String(Addresses.First());
        //return string.Join(", ", Addresses.Select(String));
    }
}
