<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:gpx="http://www.topografix.com/GPX/1/0" exclude-result-prefixes="gpx">

    <xsl:include href="extensions.xsl"/>
    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="name" select="gpx:gpx/gpx:name"/>
        <xsl:variable name="type" select="'Pairs Table'"/>
        <xsl:variable name="header" select="concat($type, ' - ', $name)"/>

        <xsl:call-template name="doctype"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="name" select="gpx:gpx/gpx:name"/>
                <xsl:with-param name="hash" select="gpx:gpx/gpx:desc"/>
                <xsl:with-param name="created" select="gpx:gpx/gpx:time"/>
                <xsl:with-param name="waypoints" select="gpx:gpx/gpx:wpt"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the body section of the HTML -->
    <xsl:template name="html_body_template">
        <xsl:param name="header" />
        <xsl:param name="name" />
        <xsl:param name="hash" />
        <xsl:param name="created" />
        <xsl:param name="waypoints" />
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr />
            <p>Name: <xsl:value-of select="$name" /></p>
            <p>Hash: <xsl:value-of select="$hash" /></p>
            <p>Created: <xsl:value-of select="$created" /></p>
            <xsl:call-template name="table">
                <xsl:with-param name="waypoints" select="$waypoints"/>
            </xsl:call-template>
            <hr />
        </body>
    </xsl:template>

    <!-- Template to create the table structure -->
    <xsl:template name="table">
        <xsl:param name="waypoints" />
        <table>
            <tr>
                <th>#</th>
                <th>Latitude</th>
                <th>Longitude</th>
                <th>Name</th>
                <th>Time</th>
                <th>Description</th>
            </tr>
            <xsl:apply-templates select="$waypoints"/>
        </table>
    </xsl:template>

    <!-- Match each waypoint and populate the table row -->
    <xsl:template match="gpx:wpt">
        <xsl:variable name="index" select="position()"/>
        <xsl:variable name="lat" select="@lat"/>
        <xsl:variable name="lon" select="@lon"/>
        <xsl:variable name="name" select="gpx:name"/>
        <xsl:variable name="time" select="gpx:time"/>
        <xsl:variable name="desc" select="gpx:desc"/>
        <xsl:call-template name="point">
            <xsl:with-param name="index" select="$index"/>
            <xsl:with-param name="lat" select="$lat"/>
            <xsl:with-param name="lon" select="$lon"/>
            <xsl:with-param name="name" select="$name"/>
            <xsl:with-param name="time" select="$time"/>
            <xsl:with-param name="desc" select="$desc"/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to create a table row -->
    <xsl:template name="point">
        <xsl:param name="index"/>
        <xsl:param name="lat"/>
        <xsl:param name="lon"/>
        <xsl:param name="name"/>
        <xsl:param name="time"/>
        <xsl:param name="desc"/>
        <tr>
            <td><xsl:number value="$index" format="1"/></td>
            <td><xsl:value-of select="$lat"/></td>
            <td><xsl:value-of select="$lon"/></td>
            <td><xsl:value-of select="$name"/></td>
            <td><xsl:value-of select="$time"/></td>
            <td><xsl:apply-templates select="$desc"/></td>
        </tr>
    </xsl:template>

    <!-- Template to match the <desc> tag -->
    <xsl:template match="gpx:desc">
        <xsl:call-template name="replace-newline">
            <xsl:with-param name="text" select="."/>
        </xsl:call-template>
    </xsl:template>

</xsl:stylesheet>
