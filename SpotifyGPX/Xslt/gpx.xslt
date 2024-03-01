<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:gpx="http://www.topografix.com/GPX/1/0" exclude-result-prefixes="gpx">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Pairs Table - <xsl:value-of select="gpx:gpx/gpx:name" /></title>
                <link rel="stylesheet" href="styles.css" />
            </head>
            <body>
                <h1>Pairs Table - <xsl:value-of select="gpx:gpx/gpx:name" /></h1>
                <hr />
                <p>Name: <xsl:value-of select="gpx:gpx/gpx:name" /></p>
                <p>Hash: <xsl:value-of select="gpx:gpx/gpx:desc" /></p>
                <p>Created: <xsl:value-of select="gpx:gpx/gpx:time" /></p>
                <table>
                    <tr>
                        <th>#</th>
                        <th>Latitude</th>
                        <th>Longitude</th>
                        <th>Name</th>
                        <th>Time</th>
                        <th>Description</th>
                    </tr>
                    <xsl:apply-templates select="gpx:gpx/gpx:wpt"/>
                </table>
                <hr />
            </body>
        </html>
    </xsl:template>

    <!-- Match each waypoint and populate the table row -->
    <xsl:template match="gpx:wpt">
        <tr>
            <td><xsl:number value="position()" format="1"/></td>
            <td><xsl:value-of select="@lat"/></td>
            <td><xsl:value-of select="@lon"/></td>
            <td><xsl:value-of select="gpx:name"/></td>
            <td><xsl:value-of select="gpx:time"/></td>
            <td><xsl:apply-templates select="gpx:desc"/></td>
        </tr>
    </xsl:template>

    <!-- Template to match the <desc> tag -->
    <xsl:template match="gpx:desc">
        <xsl:call-template name="replace-newline">
            <xsl:with-param name="text" select="."/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to recursively replace CRLF with <br> -->
    <xsl:template name="replace-newline">
        <xsl:param name="text"/>
        <xsl:choose>
            <xsl:when test="contains($text, '&#xA;')">
                <xsl:value-of select="substring-before($text, '&#xA;')"/>
                <br/>
                <xsl:call-template name="replace-newline">
                    <xsl:with-param name="text" select="substring-after($text, '&#xA;')"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$text"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>
