// SpotifyGPX by Simon Field

using Geocoding;
using SpotifyGPX.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyGPX.Api.Geocoding;

/// <summary>
/// A generic class for geocoding from a variety of API sources.
/// </summary>
/// <typeparam name="TPoint">The object type of a GPS point. <typeparamref name="TPoint"/> must implement <see cref="IApiMetadataRecordable{TIdentifier, TMetadata}"/>.</typeparam>
/// <typeparam name="TMetadata">The type of metadata holder for geocoding response data.</typeparam>
public abstract partial class GeocoderHandler<TResponse> :
    ApiMetadataHandler<IGpsPoint, Coordinate, IGpsMetadata, IGpsPoint>
{
    protected override string NameOfEntity => "location";
    protected override Func<IGpsPoint, IGpsPoint> Converter => x => x;

    /// <summary>
    /// The maximum number of coordinates to request from the API in a single batch.
    /// </summary>
    protected abstract int BatchSize { get; }

    /// <summary>
    /// The geocoder API service to use for geocoding.
    /// </summary>
    protected abstract IGeocoder Coder { get; }

    protected GeocoderHandler(List<IGpsPoint> points, StringBroadcaster bcaster) : base(points, bcaster) { }

    /// <summary>
    /// Get the metadata entry of type <typeparamref name="TMetadata"/> for a given <see cref="Coordinate"/> identifier and set of <typeparamref name="TResponse"/> addresses.
    /// </summary>
    /// <param name="identifier">The coordinate used in the API request payload.</param>
    /// <param name="addresses">The addresses the API responded with for this coordinate.</param>
    /// <returns>A metadata entry of type <typeparamref name="TMetadata"/> for the given <paramref name="identifier"/> and <paramref name="addresses"/>.</returns>
    protected abstract IGpsMetadata GetEntry(Coordinate identifier, IEnumerable<TResponse> addresses);

    protected override Dictionary<Coordinate, IGpsMetadata> GetMetadatas(List<Coordinate> identifiers)
    {
        return GetAddresses(identifiers)
            .ToDictionary(pair => pair.Key, pair => GetEntry(pair.Key, pair.Value));
    }

    private IEnumerable<KeyValuePair<Coordinate, IEnumerable<TResponse>>> GetAddresses(List<Coordinate> identifiers)
    {
        List<IEnumerable<Coordinate>> coordinateChunks = SplitIntoChunks(identifiers, BatchSize).ToList();
        int totalChunks = coordinateChunks.Count;
        BCaster.Broadcast($"Created {totalChunks} request chunks of {BatchSize} max coordinates each");

        return coordinateChunks.SelectMany((chunk, index) =>
        {
            int chunkIndex = index + 1;
            int coordinatesInChunk = chunk.Count();

            BCaster.Broadcast($"Chunk {chunkIndex} of {totalChunks}: requesting {coordinatesInChunk} coordinates from API...");

            return ChunkHandler(chunk, chunkIndex, totalChunks);
        });
    }

    /// <summary>
    /// Handle a chunk of coordinates' response addresses.
    /// </summary>
    /// <param name="chunk">The <see cref="Coordinate"/> objects comprising this request.</param>
    /// <param name="chunkIndex">The index of this chunk in a series of chunks to request.</param>
    /// <param name="totalChunks">The total number of chunks in the series of chunks to request.</param>
    /// <returns></returns>
    protected abstract IEnumerable<KeyValuePair<Coordinate, IEnumerable<TResponse>>> ChunkHandler(IEnumerable<Coordinate> chunk, int chunkIndex, int totalChunks);

    /// <summary>
    /// Get addresses from the API.
    /// </summary>
    /// <param name="coordinates">An array of <see cref="Coordinate"/> objects to reverse geocode.</param>
    /// <returns>An asynchronous <see cref="Task"/> representing the async operation to get addresses from the API.</returns>
    protected async Task<IEnumerable<KeyValuePair<Coordinate, IEnumerable<TResponse>>>> GetAddressesFromApi(IEnumerable<Coordinate> coordinates)
    {
        List<KeyValuePair<Coordinate, IEnumerable<TResponse>>> addresses = new();

        foreach (Coordinate coordinate in coordinates)
        {
            try
            {
                IEnumerable<TResponse> result = await RunSpecificGeocode(coordinate);
                addresses.Add(new KeyValuePair<Coordinate, IEnumerable<TResponse>>(coordinate, result));
            }
            catch (Exception ex)
            {
                BCaster.BroadcastError(new Exception($"Error requestion coordinate: {coordinate.ToString()}", ex));
            }
        }

        BCaster.Broadcast($"Retrieved {addresses.Count}/{coordinates.Count()} addresses from API");

        return addresses;
    }

    /// <summary>
    /// Get specific API data of type <typeparamref name="TResponse"/> for a given <see cref="Coordinate"/>.
    /// </summary>
    /// <param name="coordinate">A <see cref="Coordinate"/> to reverse geocode.</param>
    /// <returns>A task representing the async operation to get specific API data of type <typeparamref name="TResponse"/> for a given <paramref name="coordinate"/>.</returns>
    protected abstract Task<IEnumerable<TResponse>> RunSpecificGeocode(Coordinate coordinate);
}
