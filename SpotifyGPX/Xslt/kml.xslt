<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:kml="http://www.opengis.net/kml/2.2" exclude-result-prefixes="kml">

    <xsl:include href="extensions.xsl"/>
    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="name" select="kml:kml/kml:Document/kml:name"/>
        <xsl:variable name="type" select="'Pairs Table'"/>
        <xsl:variable name="header" select="concat($type, ' - ', $name)"/>

        <xsl:call-template name="doctype"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="name" select="//kml:Document/kml:name"/>
                <xsl:with-param name="hash" select="//kml:Document/kml:description"/>
                <xsl:with-param name="created" select="//kml:Document/kml:snippet"/>
                <xsl:with-param name="waypoints" select="//kml:Placemark"/>
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

    <!-- Match each placemark and populate the table row -->
    <xsl:template match="kml:Placemark">
        <xsl:variable name="index" select="position()"/>
        <xsl:variable name="coordinates" select="kml:Point/kml:coordinates"/>
        <xsl:variable name="lon" select="substring-before($coordinates, ',')"/>
        <xsl:variable name="lat" select="substring-after($coordinates, ',')"/>
        <xsl:variable name="name" select="kml:name"/>
        <xsl:variable name="time" select="kml:TimeStamp/kml:when"/>
        <xsl:variable name="desc" select="kml:description"/>
        <xsl:call-template name="row">
            <xsl:with-param name="index" select="$index"/>
            <xsl:with-param name="lat" select="$lat"/>
            <xsl:with-param name="lon" select="$lon"/>
            <xsl:with-param name="name" select="$name"/>
            <xsl:with-param name="time" select="$time"/>
            <xsl:with-param name="desc" select="$desc"/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to populate the table row -->
    <xsl:template name="row">
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

    <!-- Template to match the <description> tag -->
    <xsl:template match="kml:description">
        <xsl:call-template name="replace-newline">
            <xsl:with-param name="text" select="."/>
        </xsl:call-template>
    </xsl:template>

</xsl:stylesheet>
