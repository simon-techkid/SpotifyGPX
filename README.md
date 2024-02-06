# SpotifyGPX

Great for road trips: match GPS positions to played Spotify songs.

SpotifyGPX allows you to recount where you listened to each song of a set based on a tracked journey, placing points as close as possible to the location of each song.

## Use cases and functions

SpotifyGPX allows you to create a map of the songs you listened to on Spotify during a journey. Imagine a map that shows not just where you went, but also the soundtrack of your trip! It does this by combining two kinds of data:
 - Your Spotify listening history: It gets a list of the songs you played and when you played them from Spotify.
 - A GPX file of your journey: A GPX file is like a digital map of your travels. It includes information about your location (latitude and longitude) at different points in time.

Here's how it works:
 1. You provide SpotifyGPX with these two files: Your Spotify listening history JSON file and the GPX file of your trip.
 2. It matches up the songs with the locations: It looks at the timestamps in both files to figure out which songs you were listening to at each point along your journey.

Cool things you can do after you return from your journey:
 - Visualize your musical journeys: You can use mapping tools to see your route and the songs you listened to along the way, creating a visual memory of your trip.
 - Share your musical adventures: You can share the GPX file (or Google MyMaps link) with friends or family so they can experience your trip through your music choices.
 - Create playlists based on your travels: You can export a playlist of the songs you listened to during a particular journey, making it easy to relive those memories.

Think of it like a musical scrapbook of your travels!

## Privacy notice

SpotifyGPX is not endorsed by Spotify Technology SA. It exists only as a third-party tool that you can opt to use with the data Spotify freely provides to you. SpotifyGPX does not interact with Spotify itself in any way. It relies on user-submitted data alone.

SpotifyGPX does not exchange your data with any outside parties. In other words, the data you feed SpotifyGPX (including your original GPX tracks and Spotify data) is operated on by your computer alone.
SpotifyGPX does not modify the contents of the files you feed it. It will instead create new files representing its calculated data.

Why don't we just use the Spotify API instead of having users provide JSON? The Spotify API only provides us the [last 50 songs](https://developer.spotify.com/documentation/web-api/reference/get-recently-played) played by a user, which is rarely adequate for including their entire journey (if at all).

## Requirements

View SpotifyGPX sample data and screenshots [here](Samples/README.md) to check the compliance of your data.

### SpotifyGPX:
 - [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed
 - [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) library available
 - GPX file of a tracked journey with timings (see below)
 - JSON of Spotify history covering the journey (see below)
 - Listened to Spotify during the GPX journey (see below)

### Spotify
 - Access to the Spotify account you listened with
 - [Downloaded](https://www.spotify.com/account/privacy/) Spotify listening history JSON

### GPX tracks
 - Your tracked journey in GPX (if not GPX, [convert other formats to GPX](https://www.gpsvisualizer.com/convert_input))
 - Your GPX containing frequent `<trkpt>` objects with `lat` and `lon` attributes, and `<time>` for each

## Usage

> `SpotifyGPX <json> <gpx> [-n] [-j] [-p] [-s] [-r]`

 - `SpotifyGPX` - SpotifyGPX executable
 - **Required:** `json_file` - Path to a Spotify listening history JSON
 - **Required:** `gpx_file` - Path to a GPX file
 - *Optional:* `-n` - Do not export a `GPX` from the calculated points
 - *Optional:* `-j` - Save off the relevant part of the Spotify `json`
 - *Optional:* `-p` - Export a `xspf` playlist of the songs
 - *Optional:* `-s` - Export a `txt` list of Spotify URIs (can be copied and pasted into Spotify Desktop app playlists)
 - *Optional:* `-r` - Export a `json` report of all the data used to compile the resulting pairings

## Preparing for a journey

Ensure you take the below steps to prepare before setting off:

 1. Make sure you have access to Spotify along the journey.
 2. Use an app such as [GPSLogger (for Android)](https://github.com/mendhak/gpslogger) or [Open GPX Tracker for iOS + WatchOS](https://github.com/merlos/iOS-Open-GPX-Tracker) to track your position along the route
 3. Ensure the logging app's GPS frequency setting is high, since a song is tied to a point (recommended: 1 point every 15-30 seconds)
 5. Run a few test logging sessions. Make sure points (with UTC or UTC offset time) are created by your tracker. Compare your data to the confirmed [sample track log.](Samples/sample.gpx)

As long as there is song playback and GPS tracking running simultaneously, you will be able to use SpotifyGPX to pair the two sets of data.

## After your journey

To use SpotifyGPX, retrieve the data you tracked:

 1. [Request and download](https://www.spotify.com/account/privacy/) your `Account data` or `Extended streaming history` data `json` (see below for a comparison between the two forms)
 2. Copy the appropriate GPX files from the device you used for tracking
 3. Run SpotifyGPX with the following two files:

| Imported file | Use case | Sample |
| ------------- | -------- | ------ |
| `Gpx` | The route you took and the times where you were in each place | [GPX](Samples/sample.gpx) |
| `Json` | The songs you listened to and the times you listened to them | [JSON](Samples/sample.json) |

SpotifyGPX will allow you to:

 1. Use each of the coordinates (from the GPX) to identify (with closest possible precision) a position for a song
 2. Use the exported formats below to parse the song-point positions:

| Exported file | Use case |
| ------------- | -------- |
| `Gpx` | View a map of where each of your tracks was played using a tool like [kepler.gl](https://github.com/keplergl/kepler.gl) or [Google MyMaps](https://www.google.com/mymaps) |
| `Json` | Preserve the original Spotify data (from the original JSON) used for your journey |
| `JsonReport` | Preserve all data SpotifyGPX used to create the pairings for your journey. Includes data from Json. |
| `Txt` | Create a txt file of a Spotify URI (song link) list of the songs on your journey. [Paste](https://community.spotify.com/t5/Your-Library/how-to-paste-a-list-of-URL-s-into-a-playlist/m-p/5355978/highlight/true#M19851) into Spotify playlists on Desktop. |
| `Xspf` | Use as a playlist to visualize your journey's songs in an app such as [VLC Media Player](https://www.videolan.org/) |

## SpotifyGPX Options

### Constant values

[Options.cs](SpotifyGPX/Options.cs) contains all formats used to interpret data:

| Variable name | Type | Use case |
| ------------- | ---- | -------- |
| `Console` | `string` | Each pairing's song and point time when printed to the console |
| `ConsoleTrack` | `string` | A track's start and end times as presented to the user when there are multiple tracks  to choose from |
| `GpxInput` | `string` | Each point in your tracked journey's GPX |
| `GpxTimeStyle` | `DateTimeStyles` | Interpretation of time zone as written in GPX |
| `SpotifyFull` | `string` | `ts` objects' format within the Spotify data dump |
| `SpotifyMini` | `string` | `endTime` objects' format within the Spotify data dump |
| `SpotifyTimeStyle` | `DateTimeStyles` | Interpretation of time zone as written in JSON |
| `MinimumPlaytime` | `TimeSpan` | Minimum duration a song must have been played for to be considered |
| `ExcludeSkipped` | `bool` | Don't consider songs that the user skipped, as described by Spotify JSON |
| `MaximumAbsAccuracy` | `double?` | Don't consider a song and point pair when their times are farther apart than than this (number of seconds). Null for no restriction. |
| `GpxOutput` | `string` | The time of a pairing, as written to its GPX `<time>` |
| `DescriptionPlayedAt` | `string` | A time as written to a pairing's description |
| `DescriptionTimePlayed` | `string` | A duration as written to a pairing's description |

### Multi-track selection prompt

The following parsing options are given to you when your journey contains multiple track `<trk>` elements:

| Option | Filters by track of type |
| ------ | ------------------------ |
| The index of an individual track (in the printed list) | That track only (GPX, Gap, or Combined) |
| `[A] GPX tracks` | GPX |
| `[B] GPX tracks, and gaps between them` | GPX, Gap |
| `[C] Gaps between GPX tracks only` | Gap |
| `[D] GPX tracks and Combined track` | GPX, Combined |
| `[E] Gap tracks and Combined track` | Gap, Combined |
| `[F] GPX, Gap, and Combined tracks (everything)` | GPX, Gap, Combined |

Here is an explanation of each track type:

A track generated from:
 1. GPX - track in your original GPX file
 2. Gap - gap between tracks in your file
 3. Combined - all the points in your GPX combined into a single track (ignores track designations in provided GPX)

## Types of Data from Spotify

The differences between the types of data Spotify provides are detailed below.
`Extended streaming history` data takes longer for Spotify to send, but contains significantly more information than `Account data`.
By default, SpotifyGPX needs a song name, artist name, and end time of each song. An end time value is required at the bare minimum.

### Account data (5 days):

| JSON tag name | Description |
| ------------- | ----------- |
| `endTime` | Time the song ended |
| `trackName` | Song Name |
| `artistName` | Artist Name |
| `msPlayed` | Number of milliseconds of song playback |

### Extended streaming history (30 days):

| JSON tag name | Description |
| ------------- | ----------- |
| `ts` | This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time |
| `username` | This field is your Spotify username. |
| `platform` | This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast). |
| `ms_played` | This field is the number of milliseconds the stream was played. |
| `conn_country` | This field is the country code of the country where the stream was played (e.g. SE - Sweden). |
| `ip_addr_decrypted` | This field contains the IP address logged when streaming the track. |
| `user_agent_decrypted` | This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari) |
| `master_metadata_track_name` | This field is the name of the track. |
| `master_metadata_album_artist_name` | This field is the name of the artist, band or podcast. |
| `master_metadata_album_album_name` | This field is the name of the album of the track. |
| `spotify_track_uri` | A Spotify URI, uniquely identifying the track in the form of “spotify:track:<`base-62 string`>” |
| `episode_name` | This field contains the name of the episode of the podcast. |
| `episode_show_name` | This field contains the name of the show of the podcast. |
| `spotify_episode_uri` | A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:<`base-62 string`>” |
| `reason_start` | This field is a value telling why the track started (e.g. “trackdone”) |
| `reason_end` | This field is a value telling why the track ended (e.g. “endplay”). |
| `shuffle` | This field has the value True or False depending on if shuffle mode was used when playing the track. |
| `skipped` | This field indicates if the user skipped to the next song |
| `offline` | This field indicates whether the track was played in offline mode (“True”) or not (“False”). |
| `offline_timestamp` | This field is a timestamp of when offline mode was used, if used. |
| `incognito` | This field indicates whether the track was played in incognito mode (“True”) or not (“False”). |
