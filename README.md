# SpotifyGPX

Create GPX waypoints based on timed GPX tracks and Spotify listening history. Great for road trips!

## Example

Here is an example render using [kepler.gl](https://github.com/keplergl/kepler.gl) with the original GPX track included:

![image](Example.png)

## Requirements

For using SpotifyGPX:
 - have .NET Framework 4.7.2 installed
 - have the `Newtonsoft.Json` DLL in the running directory
 - have a GPX file of a tracked journey with timings
 - listened to Spotify during the GPX journey
 - have the JSON of Spotify history covering the journey

For use with Spotify as data source:
 - You have access to the Spotify account you listened with
 - You have [downloaded](https://www.spotify.com/account/privacy/) your Spotify listening history JSON

For use with GPX as the tracked GPS format:
 - Your journey is tracked in GPX (if not, [convert other formats to GPX](https://www.gpsvisualizer.com/convert_input))
 - The GPX contains frequent `<trkpt>` objects
 - GPX contains `lat`, `lon`, and `<time>` for each `<trkpt>`

## Usage

> `SpotifyGPX <json_file> <gpx_file> [-j] [-p] [-n]`

 - `SpotifyGPX` - SpotifyGPX executable
 - **Required:** `json_file` - Path to a Spotify listening history JSON
 - **Required:** `gpx_file` - Path to a GPX file
 - *Optional:* `-j` - Save off the relevant part of the Spotify JSON
 - *Optional:* `-p` - Export a `xspf` playlist of the songs
 - *Optional:* `-n` - Do not export a GPX from the calculated points

## Obtaining Necessary Data

**Spotify:**
 1. Ensure you listened to Spotify on the applicable journey
 2. [Download](https://www.spotify.com/account/privacy/) your `Account data` or `Extended streaming history` data JSON (this will take a few days for them to process and send)

**GPS Data:**
 1. Use an app such as [GPSLogger](https://github.com/mendhak/gpslogger) to track your position
 2. Ensure the frequency of points is high, since a song is tied to each point

## Future Additions

 - ~~JSON exporting (export the relevant part of the Spotify JSON to a new file for future reference)~~
 - ~~Playlist exporting (export a GPX of song points to a m3u or some such file)~~
 - Spotify linkage (export a series of spotify URI so these can be pasted into Spotify)
