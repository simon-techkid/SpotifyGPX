// SpotifyGPX by Simon Field

using Geocoding;
using Geocoding.Google;
using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyGPX.Api.Geocoding.Google;

public partial class GeocoderGoogle : GeocoderHandler<GoogleAddress>
{
    private readonly Stopwatch stopWatch;
    protected override IGeocoder Coder { get; }
    protected override string BroadcasterPrefix => "GOOGLE";
    protected override string NameOfEntity => "Google location";

    public GeocoderGoogle(List<IGpsPoint> points, StringBroadcaster bcaster) : base(points, bcaster)
    {
        Coder = new GoogleGeocoder(API_KEY);
        stopWatch = new Stopwatch();
    }

    protected override IGpsMetadata GetEntry(Coordinate identifier, IEnumerable<GoogleAddress> addresses)
    {
        return new GeocodedGoogleEntry
        {
            Addresses = addresses
        };
    }

    protected async override Task<IEnumerable<GoogleAddress>> RunSpecificGeocode(Coordinate coord)
    {
        return await (Coder as GoogleGeocoder ?? throw new Exception("Coder is not a GoogleGeocoder"))
            .ReverseGeocodeAsync(coord.Latitude, coord.Longitude);
    }

    protected override IEnumerable<KeyValuePair<Coordinate, IEnumerable<GoogleAddress>>> ChunkHandler(IEnumerable<Coordinate> chunk, int chunkIndex, int totalChunks)
    {
        stopWatch.Start();

        var chunkAddresses = GetAddressesFromApi(chunk).GetAwaiter().GetResult();

        stopWatch.Stop();

        foreach (KeyValuePair<Coordinate, IEnumerable<GoogleAddress>> pair in chunkAddresses)
        {
            yield return pair;
        }

        // Ensure we respect the rate limit of BatchSize
        RespectRateLimit(chunkIndex, totalChunks);
        stopWatch.Reset();
    }

    private void RespectRateLimit(int chunkIndex, int totalChunks)
    {
        if (stopWatch.ElapsedMilliseconds < 60000 && chunkIndex < totalChunks)
        {
            var delay = 60000 - (int)stopWatch.ElapsedMilliseconds;
            BCaster.Broadcast($"Waiting for {delay}ms to respect rate limit...");
            Thread.Sleep(delay);
        }
    }
}
