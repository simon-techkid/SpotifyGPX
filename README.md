# SpotifyGPX

Create GPX waypoints based on timed GPX tracks and Spotify listening history. Great for road trips!

## Usage

**SpotifyGPX requires that you:**

 - have NET 6.0 installed
 - have `Newtonsoft.Json` DLL in the running directory
 - have a GPX file of a tracked journey with timings
 - listened to Spotify during the GPX journey

**Command Syntax:**

> `SpotifyGPX <json_file> <gpx_file>`

 - `SpotifyGPX` - SpotifyGPX executable
 - `json_file` - Path to a Spotify listening history JSON
 - `gpx_file` - Path to a GPX file

## Spotify

**Requirements:**
 - You have access to the Spotify account you listened with
 - You have [downloaded](https://www.spotify.com/account/privacy/) your Spotify listening history JSON
 - `Newtonsoft.Json` is required to interpret the JSON data with SpotifyGPX

**Process:**
 1. Ensure you listened to Spotify on the applicable journey
 2. [Download](https://www.spotify.com/account/privacy/) your listening history JSON (this will take a few days)

## GPS Data

**Requirements:**
 - Your journey is tracked in GPX (if not, [convert to GPX](https://www.gpsvisualizer.com/convert_input))
 - The GPX contains frequent `<trkpt>` objects
 - GPX contains `lat`, `lon`, and `<time>` for each `<trkpt>`

**Process:**
 1. Use an app such as [GPSLogger](https://github.com/mendhak/gpslogger) to track your position
 2. Ensure the frequency of points is high, since a song is tied to each point

## Future Additions

 - JSON exporting (export the relevant part of the Spotify JSON to a new file for future reference)
 - Playlist exporting (export a GPX of song points to a m3u or some such file)
 - Spotify linkage (export a series of spotify URI so these can be reimported
