# SpotifyGPX

Create GPX waypoints based on timed GPX tracks and Spotify listening history. Great for road trips!

## Example

Here is an example render using [kepler.gl](https://github.com/keplergl/kepler.gl) with the original GPX track included:

![image](Example.png)

## Requirements

You must have the following:

### for SpotifyGPX:
 - .NET 6.0 installed
 - the `Newtonsoft.Json` library in the running directory
 - a GPX file of a tracked journey with timings
 - listened to Spotify during the GPX journey
 - the JSON of Spotify history covering the journey

### for using Spotify as data source:
 - access to the Spotify account you listened with
 - [downloaded](https://www.spotify.com/account/privacy/) your Spotify listening history JSON

### for using GPX tracks as the source format:
 - your journey, tracked in GPX (if not, [convert other formats to GPX](https://www.gpsvisualizer.com/convert_input))
 - the GPX, containing frequent `<trkpt>` objects, as well as `lat`, `lon`, and `<time>`, for each

## Usage

> `SpotifyGPX <json_file> <gpx_file> [-j] [-p] [-n]`

 - `SpotifyGPX` - SpotifyGPX executable
 - **Required:** `json_file` - Path to a Spotify listening history JSON
 - **Required:** `gpx_file` - Path to a GPX file
 - *Optional:* `-n` - Do not export a GPX from the calculated points
 - *Optional:* `-j` - Save off the relevant part of the Spotify JSON
 - *Optional:* `-p` - Export a `xspf` playlist of the songs
 - *Optional:* `-s` - Export a `txt` list of Spotify URIs and copy it to clipboard (can be pasted into Spotify Desktop app playlists)
 - *Optional:* `-g` - Enable point prediction for this run. See below for guidance. **Cannot be used with `-n`**

## Obtaining Necessary Data

### Spotify:
 1. Ensure you listened to Spotify on the applicable journey
 2. [Download](https://www.spotify.com/account/privacy/) your `Account data` or `Extended streaming history` data JSON (this will take a few days for them to process and send)

### GPS Data:
 1. Use an app such as [GPSLogger](https://github.com/mendhak/gpslogger) to track your position
 2. Ensure the frequency of points is high, since a song is tied to each point

## Types of Data from Spotify

Below are some differences between what each type of data Spotify offers has.
`Extended streaming history` data takes longer to get, but contains significantly more information than `Account data`

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

## Point Prediction

### What is it?

SpotifyGPX has a built-in point prediction class that allows multiple songs spanning the same point to be distributed along the route, as opposed to overlapping one another.

### When should it be used?

This is useful if there is a gap in GPS track data spanning longer than one song (ie. tunnel passage with no GPS reception but continued song playback) or GPS track points captured otherwise too infrequently.

### How is it accomplished?

By identifying the first point with a duplicate coordinate, and the last point of the same coordinate, and equally spacing out all implicated points between the beginning and end of the missing path.

### Modes:
 1. *Default* - No point prediction. Multiple songs spanning same point overlap first point
 2. *Without KML* - Equidistant point prediction. Multiple songs spanning same point distributed equally until next unique coordinate
 3. *With KML* - Provided KML of road path, multiple songs spanning same point placed on the coordinates of the road from the KML

**Provide KML Path** for use with road path:

Example: `route.gpx`
KML path of road from [Google MyMaps](https://www.google.com/mymaps): `route_Spotify.kml`

**Note:** If this file does not exist, and program is run with `-g`, equidistant placement will be used.

### Example Comparison:

![image](PointPrediction.png)

## Console Example

![image](Console.PNG)