<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Pairs Summary</title>
                <link rel="stylesheet" href="styles.css" />
            </head>
            <body>
                <h1>Pairs Summary</h1>
                <hr />
                <h2>All Tracks</h2>
                <p>Track Count: <xsl:value-of select="count(//Root/Root[position() > 1])"/></p>
                <p>Pair Count: <xsl:value-of select="count(//Root/Root[position() > 1]/*[position() > 2])"/>/<xsl:value-of select="//Root/Root[1]/Total"/></p>
                <p>Hash: <xsl:value-of select="//Root/Root[1]/SHA256Hash"/></p>
                <hr />
                <xsl:apply-templates select="//Root/Root[position() > 1]"/>
            </body>
        </html>
    </xsl:template>

    <!-- Create an HTML section & table for each track and its pairs -->
    <xsl:template match="Root">
        <xsl:variable name="currentRoot" select="."/>
        <h2>Track <xsl:value-of select="position()"/>: <xsl:value-of select="TrackInfo/Name"/></h2>
        <p>Pair Count: <xsl:value-of select="count($currentRoot/*[position() > 2])"/>/<xsl:value-of select="Count"/></p>
        <p>Index: <xsl:value-of select="TrackInfo/Index"/></p>
        <p>Name: <xsl:value-of select="TrackInfo/Name"/></p>
        <p>Type: <xsl:value-of select="TrackInfo/Type"/></p>
        <table>
            <tr>
                <th>#</th>
                <th>Index</th>
                <th>Title - Artist</th>
                <th>Point Time</th>
                <th>Accuracy</th>
                <th>Song Time</th>                
                <th>Track URL</th>
            </tr>
            <xsl:for-each select="$currentRoot/*[position() > 2]">
                <tr>
                    <td><xsl:value-of select="position()"/></td>
                    <td><xsl:value-of select="Index"/></td>
                    <td><xsl:value-of select="Song/master_metadata_track_name"/> - <xsl:value-of select="Song/master_metadata_album_artist_name"/></td>
                    <td><xsl:value-of select="PointTime"/></td>
                    <td><xsl:value-of select="Accuracy"/></td>
                    <td><xsl:value-of select="SongTime"/></td>
                    <td><a href="{Song/SGPX_Song_URL}">Listen on Spotify</a></td>
                </tr>
            </xsl:for-each>
        </table>
        <hr />
    </xsl:template>

</xsl:stylesheet>
