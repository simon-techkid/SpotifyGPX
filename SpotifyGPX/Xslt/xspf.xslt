<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xspf="http://xspf.org/ns/0/" exclude-result-prefixes="xspf">
    
    <xsl:output method="html" indent="yes" encoding="UTF-8"/>
    
    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Spotify Table - <xsl:value-of select="xspf:playlist/xspf:title"/></title>
                <link rel="stylesheet" href="styles.css"/>
            </head>
            <body>
                <h1>Spotify Table - <xsl:value-of select="xspf:playlist/xspf:title"/></h1>
                <hr />
                <p>Name: <xsl:value-of select="xspf:playlist/xspf:title"/></p>
                <p>Creator: <xsl:value-of select="xspf:playlist/xspf:creator"/></p>
                <p>Comment: <xsl:value-of select="xspf:playlist/xspf:annotation"/></p>
                <p>Hash: <xsl:value-of select="xspf:playlist/xspf:identifier"/></p>
                <p>Created: <xsl:value-of select="xspf:playlist/xspf:date"/></p>
                <table>
                    <tr>
                        <th>#</th>
                        <th>Artist</th>
                        <th>Title</th>
                        <th>Time</th>
                        <th>Duration Played</th>
                    </tr>
                    <xsl:apply-templates select="xspf:playlist/xspf:trackList/xspf:track"/>
                    <tr>
                    <td colspan="4">Total Duration</td>
                        <td><xsl:call-template name="totalDuration"/></td>
                    </tr>
                </table>
                <hr />
            </body>
        </html>
    </xsl:template>
    
    <!-- Match each track and populate the table row -->
    <xsl:template match="xspf:track">
        <tr>
            <td><xsl:number value="position()" format="1"/></td>
            <td><xsl:value-of select="xspf:creator"/></td>
            <td><xsl:value-of select="xspf:title"/></td>
            <td><xsl:value-of select="xspf:annotation"/></td>
            <td><xsl:call-template name="formatDuration">
                <xsl:with-param name="milliseconds" select="xspf:duration"/>
            </xsl:call-template>
            </td>
        </tr>
    </xsl:template>

    <!-- Template to format duration from milliseconds to human-readable format -->
    <xsl:template name="formatDuration">
        <xsl:param name="milliseconds"/>
        <xsl:variable name="seconds" select="floor($milliseconds div 1000)"/>
        <xsl:variable name="minutes" select="floor($seconds div 60)"/>
        <xsl:variable name="hours" select="floor($minutes div 60)"/>
        <xsl:value-of select="concat(format-number($hours, '00'), ':', format-number($minutes mod 60, '00'), ':', format-number($seconds mod 60, '00'))"/>
    </xsl:template>

    <!-- Template to calculate total duration -->
    <xsl:template name="totalDuration">
        <xsl:variable name="totalMilliseconds" select="sum(xspf:playlist/xspf:trackList/xspf:track/xspf:duration)"/>
        <xsl:call-template name="formatDuration">
            <xsl:with-param name="milliseconds" select="$totalMilliseconds"/>
        </xsl:call-template>
    </xsl:template>

</xsl:stylesheet>
