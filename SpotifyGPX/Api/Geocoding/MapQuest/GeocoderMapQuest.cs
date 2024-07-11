// SpotifyGPX by Simon Field

using Geocoding;
using Geocoding.MapQuest;
using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyGPX.Api.Geocoding.MapQuest;

public partial class GeocoderMapQuest : GeocoderHandler<Address>
{
    protected override IGeocoder Coder { get; }
    protected override string BroadcasterPrefix => "MAPQUEST";
    protected override string NameOfEntity => "MapQuest location";

    public GeocoderMapQuest(List<IGpsPoint> points, StringBroadcaster bcaster) : base(points, bcaster)
    {
        Coder = new MapQuestGeocoder(API_KEY);
    }

    protected override IGpsMetadata GetEntry(Coordinate identifier, IEnumerable<Address> addresses)
    {
        return new GeocodedMapQuestEntry
        {
            Addresses = addresses
        };
    }

    protected override async Task<IEnumerable<Address>> RunSpecificGeocode(Coordinate coordinate)
    {
        return await (Coder as MapQuestGeocoder ?? throw new Exception(""))
            .ReverseGeocodeAsync(coordinate.Latitude, coordinate.Longitude);
    }

    protected override IEnumerable<KeyValuePair<Coordinate, IEnumerable<Address>>> ChunkHandler(IEnumerable<Coordinate> chunk, int chunkIndex, int totalChunks)
    {
        return GetAddressesFromApi(chunk.ToArray()).GetAwaiter().GetResult();
    }
}
