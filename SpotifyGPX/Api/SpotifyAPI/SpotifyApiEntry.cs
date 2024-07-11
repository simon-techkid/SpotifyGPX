// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Api.SpotifyAPI;

/// <summary>
/// A record of a Spotify song's metadata.
/// </summary>
public partial struct SpotifyApiEntry
{
    /// <summary>
    /// The Spotify ID for the track.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The URL for the track.
    /// </summary>
    public readonly string Url => $"https://open.spotify.com/track/{Id}";

    /// <summary>
    /// The name of the track.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The track length in milliseconds.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// <see cref="Duration"/> parsed to a <see cref="TimeSpan"/>.
    /// </summary>
    public readonly TimeSpan DurationSpan => TimeSpan.FromMilliseconds(Duration);

    /// <summary>
    /// The popularity of the track. The value will be between 0 and 100, with 100 being the most popular.
    /// The popularity of a track is a value between 0 and 100, with 100 being the most popular.
    /// The popularity is calculated by algorithm and is based, in the most part, on the total number of plays the track has had and how recent those plays are.
    /// Generally speaking, songs that are being played a lot now will have a higher popularity than songs that were played a lot in the past.
    /// Duplicate tracks (e.g.the same track from a single and an album) are rated independently.
    /// Artist and album popularity is derived mathematically from track popularity.
    /// Note: the popularity value may lag actual popularity by a few days: the value is not updated in real time.
    /// </summary>
    public int Popularity { get; set; }

    /// <summary>
    /// The artists who performed the track.
    /// Each artist object includes a link in href to more detailed information about the artist.
    /// </summary>
    public List<Artist> Artists { get; set; }

    /// <summary>
    /// The album on which the track appears.
    /// The album object includes a link in href to full information about the album.
    /// </summary>
    public Album Album { get; set; }

    /// <summary>
    /// Get a string containing the names of the artists who performed the track.
    /// </summary>
    /// <returns>A string, with each contributing artist separated by a comma.</returns>
    public readonly string GetArtists() => string.Join(", ", Artists.Select(a => a.Name));

    /// <summary>
    /// Get a string containing the genres of the artists who performed the track.
    /// </summary>
    /// <returns>A string, with each contributing artist's genres separated by commas.</returns>
    public readonly string GetGenres() => string.Join(", ", string.Join(", ", Artists.Where(artist => artist.Genres.Length > 0).Select(a => string.Join(", ", a.Genres.Where(genre => !string.IsNullOrEmpty(genre)))).Distinct()));
}

/// <summary>
/// A record of a Spotify artist's metadata.
/// </summary>
public struct Artist
{
    /// <summary>
    /// The Spotify ID for the artist.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The URL for the artist.
    /// </summary>
    public readonly string Url => $"https://open.spotify.com/artist/{Id}";

    /// <summary>
    /// The name of the artist.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A list of the genres the artist is associated with. If not yet classified, the array is empty.
    /// </summary>
    public string[] Genres { get; set; }
}

/// <summary>
/// A record of a Spotify album's metadata.
/// </summary>
public struct Album
{
    /// <summary>
    /// The Spotify ID for the album.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The Spotify URL for the album.
    /// </summary>
    public readonly string Url => $"https://open.spotify.com/album/{Id}";

    /// <summary>
    /// The name of the album. In case of an album takedown, the value may be an empty string.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The date the album was first released. Depending on the precision, it might be shown as YYYY, YYYY-MM, or YYYY-MM-DD.
    /// </summary>
    public string ReleaseDate { get; set; }
}
