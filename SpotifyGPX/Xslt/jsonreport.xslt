<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:include href="extensions.xsl"/>
    <xsl:output method="html" version="1.0" encoding="UTF-8" omit-xml-declaration="no" indent="yes" media-type="text/html"/>

    <!-- The position of the header amongst the JsonReport objects -->
    <xsl:variable name="headerPosition" select="number(1)"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Pairs Summary'"/>
        <xsl:variable name="header" select="$type"/>

        <!-- Calculate actual object counts -->
        <xsl:variable name="pairsInFile" select="count(//Root/Object[position() > $headerPosition]/Track/*)"/>
        <xsl:variable name="trackCount" select="count(//Root/Object[position() > $headerPosition])"/>

        <!-- Get values from the JsonReport header -->
        <xsl:variable name="createdOn" select="//Root/Object[$headerPosition]/Created"/>
        <xsl:variable name="pairsExpected" select="//Root/Object[$headerPosition]/Total"/>
        <xsl:variable name="hash" select="//Root/Object[$headerPosition]/SHA256Hash"/>

        <!-- Create fraction for actual number of pairs in the file to the number of pairs expected according to the header -->
        <xsl:variable name="pairsComp" select="concat($pairsInFile, '/', $pairsExpected)"/>

        <xsl:call-template name="doctype"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="createdOn" select="$createdOn"/>
                <xsl:with-param name="track_count" select="$trackCount"/>
                <xsl:with-param name="pairComposition" select="$pairsComp"/>
                <xsl:with-param name="hash" select="$hash"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the body section of the HTML -->
    <xsl:template name="html_body_template">
        <xsl:param name="header" />
        <xsl:param name="createdOn"/>
        <xsl:param name="track_count"/>
        <xsl:param name="pairComposition"/>
        <xsl:param name="hash"/>
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr />
            <h2>All Tracks</h2>
            <p>Created On: <xsl:value-of select="$createdOn"/></p>
            <p>Track Count: <xsl:value-of select="$track_count"/></p>
            <p>Pair Count: <xsl:value-of select="$pairComposition"/></p>
            <p>Hash: <xsl:value-of select="$hash"/></p>
            <hr />
            <xsl:apply-templates select="//Root/Object[position() > $headerPosition]"/>
        </body>
    </xsl:template>

    <!-- Template for generating a track block -->
    <xsl:template match="//Root/Object">
        <xsl:apply-templates select="." mode="header"/>
        <xsl:apply-templates select="." mode="count"/>
        <xsl:apply-templates select="." mode="trackInfo"/>
        <xsl:apply-templates select="." mode="pairTable"/>
        <hr />
    </xsl:template>

    <!-- Template for creating the header text of a track -->
    <xsl:template match="//Root/Object" mode="header">
        <xsl:variable name="index" select="position()"/>
        <xsl:variable name="name" select="TrackInfo/Name"/>
        <xsl:variable name="header" select="concat('Track ', $index, ': ', $name)"/>
        <h2><xsl:value-of select="$header"/></h2>
    </xsl:template>

    <!-- Template for displaying track pair count -->
    <xsl:template match="//Root/Object" mode="count">
        <xsl:variable name="pairCount" select="count(Track/*)"/>
        <xsl:variable name="pairsExpected" select="Count"/>
        <xsl:variable name="pairsComp" select="concat('Pair Count: ', $pairCount, '/', $pairsExpected)"/>
        <p><xsl:value-of select="$pairsComp"/></p>
    </xsl:template>

    <!-- Template for displaying TrackInfo -->
    <xsl:template match="//Root/Object" mode="trackInfo">
        <xsl:apply-templates select="TrackInfo"/>
    </xsl:template>

    <!-- Template for assembling TrackInfo -->
    <xsl:template match="TrackInfo">
        <xsl:variable name="index" select="./Index"/>
        <xsl:variable name="name" select="./Name"/>
        <xsl:variable name="type" select="./Type"/>
        <xsl:variable name="typeFriendly">
            <xsl:apply-templates select="$type"/>
        </xsl:variable>
        <xsl:variable name="typeComposition" select="concat($typeFriendly, ' (', $type, ')' )"/>
        <p>Index: <xsl:value-of select="$index"/></p>
        <p>Name: <xsl:value-of select="$name"/></p>
        <p>Type: <xsl:value-of select="$typeComposition"/></p>
    </xsl:template>

    <!-- Template for getting a string for the type of a track -->
    <xsl:template match="TrackInfo/Type">
        <xsl:choose>
            <xsl:when test=". = 0">GPS</xsl:when>
            <xsl:when test=". = 1">Gap</xsl:when>
            <xsl:when test=". = 2">Combined</xsl:when>
            <xsl:otherwise>Unknown</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!-- Template for generating the pair table -->
    <xsl:template match="//Root/Object" mode="pairTable">
        <table>
            <tr>
                <th>#</th>
                <th>Description</th>
                <th>Index</th>
                <th>Title - Artist</th>
                <th>Point Time</th>
                <th>Accuracy</th>
                <th>Song Time</th>
            </tr>
            <xsl:apply-templates select="Track/Object"/>
        </table>
    </xsl:template>

    <!-- Template for generating table rows for each pair -->
    <xsl:template match="Track/Object">
        <tr>
            <td><xsl:value-of select="position()"/></td>
            <td><xsl:apply-templates select="Description"/></td>
            <td><xsl:value-of select="Index"/></td>
            <td><xsl:value-of select="concat(Song/Song_Name, ' - ', Song/Song_Artist)"/></td>
            <td><xsl:value-of select="PointTime"/></td>
            <td><xsl:value-of select="Accuracy"/></td>
            <td><xsl:value-of select="SongTime"/></td>
        </tr>
    </xsl:template>

    <!-- Template to match the <desc> tag -->
    <xsl:template match="Description">
        <xsl:call-template name="replace-newline">
            <xsl:with-param name="text" select="."/>
        </xsl:call-template>
    </xsl:template>

</xsl:stylesheet>
