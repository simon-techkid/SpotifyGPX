<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Pairs Summary'"/>
        <xsl:variable name="header" select="$type"/>
        <xsl:variable name="stylesheet" select="'styles.css'"/>
        <xsl:variable name="pairsInFile" select="count(//Root/Root[position() > 1]/*[position() > 2])"/>
        <xsl:variable name="headerPosition" select="number(1)"/>
        <xsl:variable name="pairsExpected" select="//Root/Root[$headerPosition]/Total"/>
        <xsl:variable name="pairsComp" select="concat($pairsInFile, '/', $pairsExpected)"/>
        <xsl:variable name="trackCount" select="count(//Root/Root[position() > $headerPosition])"/>
        <xsl:variable name="hash" select="//Root/Root[$headerPosition]/SHA256Hash"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
                <xsl:with-param name="stylesheet" select="$stylesheet"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="track_count" select="$trackCount"/>
                <xsl:with-param name="pairComposition" select="$pairsComp"/>
                <xsl:with-param name="hash" select="$hash"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the head section of the HTML -->
    <xsl:template name="html_head_template">
        <xsl:param name="title" />
        <xsl:param name="stylesheet" />
        <head>
            <title><xsl:value-of select="$title" /></title>
            <link rel="stylesheet" href="{$stylesheet}" />
        </head>
    </xsl:template>

    <!-- Template to create the body section of the HTML -->
    <xsl:template name="html_body_template">
        <xsl:param name="header" />
        <xsl:param name="track_count"/>
        <xsl:param name="pairComposition"/>
        <xsl:param name="hash"/>
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr />
            <h2>All Tracks</h2>
            <p>Track Count: <xsl:value-of select="$track_count"/></p>
            <p>Pair Count: <xsl:value-of select="$pairComposition"/></p>
            <p>Hash: <xsl:value-of select="$hash"/></p>
            <hr />
            <xsl:apply-templates select="//Root/Root[position() > 1]" mode="processRoot"/>
        </body>
    </xsl:template>

    <!-- Template to handle each Root element -->
    <xsl:template match="Root" mode="processRoot">
        <xsl:call-template name="processRoot">
            <xsl:with-param name="currentRoot" select="."/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to process the Root element with proper scoping -->
    <xsl:template name="processRoot">
        <xsl:param name="currentRoot"/>
        <xsl:call-template name="trackHeader"/>
        <xsl:call-template name="pairCount">
            <xsl:with-param name="currentRoot" select="$currentRoot"/>
        </xsl:call-template>
        <xsl:call-template name="trackInfo"/>
        <xsl:call-template name="songTable">
            <xsl:with-param name="currentRoot" select="$currentRoot"/>
        </xsl:call-template>
        <hr />
    </xsl:template>

    <!-- Template for generating the track header -->
    <xsl:template name="trackHeader">
        <xsl:variable name="index" select="position()"/>
        <xsl:variable name="name" select="TrackInfo/Name"/>
        <xsl:variable name="header" select="concat('Track ', $index, ': ', $name)"/>
        <h2><xsl:value-of select="$header"/></h2>
    </xsl:template>

    <!-- Template for displaying the pair count -->
    <xsl:template name="pairCount">
        <xsl:param name="currentRoot"/>
        <xsl:variable name="pairCount" select="count($currentRoot/*[position() > 2])"/>
        <xsl:variable name="pairsExpected" select="Count"/>
        <xsl:variable name="pairsComp" select="concat('Pair Count: ', $pairCount, '/', $pairsExpected)"/>
        <p><xsl:value-of select="$pairsComp"/></p>
    </xsl:template>

    <!-- Template for displaying track information -->
    <xsl:template name="trackInfo">
        <xsl:variable name="index" select="TrackInfo/Index"/>
        <xsl:variable name="name" select="TrackInfo/Name"/>
        <xsl:variable name="type" select="TrackInfo/Type"/>
        <xsl:variable name="typeFriendly">
            <xsl:call-template name="typePicker">
                <xsl:with-param name="typeNumeric" select="$type"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="typeComposition" select="concat($typeFriendly, ' (', $type, ')' )"/>
        <p>Index: <xsl:value-of select="$index"/></p>
        <p>Name: <xsl:value-of select="$name"/></p>
        <p>Type: <xsl:value-of select="$typeComposition"/></p>
    </xsl:template>

    <xsl:template name="typePicker">
        <xsl:param name="typeNumeric"/>
        <xsl:choose>
            <xsl:when test="$typeNumeric = 0">GPX</xsl:when>
            <xsl:when test="$typeNumeric = 1">Gap</xsl:when>
            <xsl:when test="$typeNumeric = 2">Combined</xsl:when>
            <xsl:otherwise>Unknown</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!-- Template for generating the song table -->
    <xsl:template name="songTable">
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
            <td><xsl:value-of select="concat(Song/master_metadata_track_name, ' - ', Song/master_metadata_album_artist_name)"/></td>
            <td><xsl:value-of select="PointTime"/></td>
            <td><xsl:value-of select="Accuracy"/></td>
            <td><xsl:value-of select="SongTime"/></td>
            <td><a href="{Song/SGPX_Song_URL}">Listen on Spotify</a></td>
        </tr>
    </xsl:template>

</xsl:stylesheet>
