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
                <!-- Call the main template with the currentRoot parameter -->
                <xsl:apply-templates select="//Root/Root[position() > 1]" mode="processRoot"/>
            </body>
        </html>
    </xsl:template>

    <!-- Template to handle each Root element -->
    <xsl:template match="Root" mode="processRoot">
        <!-- Pass the currentRoot as parameter to the processRoot template -->
        <xsl:call-template name="processRoot">
            <xsl:with-param name="currentRoot" select="."/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to process the Root element with proper scoping -->
    <xsl:template name="processRoot">
        <!-- Define the parameter -->
        <xsl:param name="currentRoot"/>
        <xsl:call-template name="trackHeader"/>
        <!-- Pass the currentRoot as parameter to the pairCount template -->
        <xsl:call-template name="pairCount">
            <xsl:with-param name="currentRoot" select="$currentRoot"/>
        </xsl:call-template>
        <xsl:call-template name="trackInfo"/>
        <!-- Pass the currentRoot as parameter to the songTable template -->
        <xsl:call-template name="songTable">
            <xsl:with-param name="currentRoot" select="$currentRoot"/>
        </xsl:call-template>
        <hr />
    </xsl:template>

    <!-- Template for generating the track header -->
    <xsl:template name="trackHeader">
        <h2>Track <xsl:value-of select="position()"/>: <xsl:value-of select="TrackInfo/Name"/></h2>
    </xsl:template>

    <!-- Template for displaying the pair count -->
    <xsl:template name="pairCount">
        <!-- Define the parameter -->
        <xsl:param name="currentRoot"/>
        <p>Pair Count: <xsl:value-of select="count($currentRoot/*[position() > 2])"/>/<xsl:value-of select="Count"/></p>
    </xsl:template>

    <!-- Template for displaying track information -->
    <xsl:template name="trackInfo">
        <p>Index: <xsl:value-of select="TrackInfo/Index"/></p>
        <p>Name: <xsl:value-of select="TrackInfo/Name"/></p>
        <p>Type:
            <xsl:choose>
                <xsl:when test="TrackInfo/Type = 0">GPX</xsl:when>
                <xsl:when test="TrackInfo/Type = 1">Gap</xsl:when>
                <xsl:when test="TrackInfo/Type = 2">Combined</xsl:when>
                <xsl:otherwise>Unknown</xsl:otherwise>
            </xsl:choose>
            (<xsl:value-of select="TrackInfo/Type"/>)
        </p>
    </xsl:template>

    <!-- Template for generating the song table -->
    <xsl:template name="songTable">
        <!-- Define the parameter -->
        <xsl:param name="currentRoot"/>
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
            <xsl:apply-templates select="$currentRoot/*[position() > 2]"/>
        </table>
    </xsl:template>

    <!-- Template for generating table rows for each song -->
    <xsl:template match="Root/*[position() > 2]">
        <tr>
            <td><xsl:value-of select="position()"/></td>
            <td><xsl:value-of select="Index"/></td>
            <td><xsl:value-of select="Song/master_metadata_track_name"/> - <xsl:value-of select="Song/master_metadata_album_artist_name"/></td>
            <td><xsl:value-of select="PointTime"/></td>
            <td><xsl:value-of select="Accuracy"/></td>
            <td><xsl:value-of select="SongTime"/></td>
            <td><a href="{Song/SGPX_Song_URL}">Listen on Spotify</a></td>
        </tr>
    </xsl:template>

</xsl:stylesheet>
