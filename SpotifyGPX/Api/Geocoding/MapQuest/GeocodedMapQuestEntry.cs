// SpotifyGPX by Simon Field

using Geocoding;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Api.Geocoding.MapQuest;

public struct GeocodedMapQuestEntry : IGpsMetadata
{
    public IEnumerable<Address> Addresses { get; set; }

    public static string String(Address address)
    {
        StringBuilder sb = new();

        sb.AppendLine("Data Provider: {0}", address.Provider);
        sb.AppendLine("Address: {0}", address.FormattedAddress);

        return sb.ToString();
    }

    public readonly string Stringify()
    {
        return String(Addresses.First());
        //return string.Join(", ", Addresses.Select(String));
    }
}
