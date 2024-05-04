<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xspf="http://xspf.org/ns/0/" exclude-result-prefixes="xspf">

    <xsl:include href="extensions.xsl"/>
    <xsl:output method="html" version="1.0" encoding="UTF-8" omit-xml-declaration="no" indent="yes" media-type="text/html"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="name" select="xspf:playlist/xspf:title"/>
        <xsl:variable name="type" select="'Songs Table'"/>
        <xsl:variable name="header" select="concat($type, ' - ', $name)"/>

        <xsl:call-template name="doctype"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="name" select="xspf:playlist/xspf:title"/>
                <xsl:with-param name="creator" select="xspf:playlist/xspf:creator"/>
                <xsl:with-param name="comment" select="xspf:playlist/xspf:annotation"/>
                <xsl:with-param name="hash" select="xspf:playlist/xspf:identifier"/>
                <xsl:with-param name="created" select="xspf:playlist/xspf:date"/>
                <xsl:with-param name="tracks" select="xspf:playlist/xspf:trackList"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the body section of the HTML -->
    <xsl:template name="html_body_template">
        <xsl:param name="header"/>
        <xsl:param name="name"/>
        <xsl:param name="creator"/>
        <xsl:param name="comment"/>
        <xsl:param name="hash"/>
        <xsl:param name="created"/>
        <xsl:param name="tracks"/>
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr />
            <p>Name: <xsl:value-of select="$name"/></p>
            <p>Creator: <xsl:value-of select="$creator"/></p>
            <p>Comment: <xsl:value-of select="$comment"/></p>
            <p>Hash: <xsl:value-of select="$hash"/></p>
            <p>Created: <xsl:value-of select="$created"/></p>
            <xsl:apply-templates select="$tracks"/>
            <hr />
        </body>
    </xsl:template>

    <!-- Template to create the table structure -->
    <xsl:template match="xspf:trackList">
        <table>
            <tr>
                <th>#</th> <!-- 1 -->
                <th>Artist</th> <!-- 2 -->
                <th>Album</th> <!-- 3 -->
                <th>Title</th> <!-- 4 -->
                <th>Time</th> <!-- 5 -->
                <th>Link</th> <!-- 6 -->
                <th>Played</th> <!-- 7 -->
            </tr>
            <xsl:apply-templates select="xspf:track"/>
            <xsl:call-template name="sumRow">
                <xsl:with-param name="durations" select="xspf:track/xspf:duration"/>
                <xsl:with-param name="colNo" select="7"/>
            </xsl:call-template>
        </table>
    </xsl:template>

    <!-- Sum row containing the sum of a collection of durations -->
    <xsl:template name="sumRow">
        <xsl:param name="durations"/>
        <xsl:param name="colNo"/>
        <xsl:variable name="spanColumn" select="($colNo - 1)"/>
        <tr>
            <td colspan="{$spanColumn}">Total Duration</td>
            <td>
                <xsl:call-template name="totalDuration">
                    <xsl:with-param name="durations" select="$durations"/>
                </xsl:call-template>
            </td>
        </tr>
    </xsl:template>

    <!-- Match each track and populate the table row -->
    <xsl:template match="xspf:track">
        <xsl:variable name="index" select="position()"/>
        <xsl:variable name="artist" select="xspf:creator"/>
        <xsl:variable name="album" select="xspf:album"/>
        <xsl:variable name="title" select="xspf:title"/>
        <xsl:variable name="time" select="xspf:annotation"/>
        <xsl:variable name="link"><xsl:apply-templates select="xspf:link"/></xsl:variable>
        <xsl:variable name="played"><xsl:apply-templates select="xspf:duration"/></xsl:variable>
        <tr>
            <td><xsl:number value="$index" format="1"/></td>
            <td><xsl:value-of select="$artist"/></td>
            <td><xsl:value-of select="$album"/></td>
            <td><xsl:value-of select="$title"/></td>
            <td><xsl:value-of select="$time"/></td>
            <td><xsl:copy-of select="$link"/></td>
            <td><xsl:value-of select="$played"/></td>
        </tr>
    </xsl:template>

    <!-- Format a single hyperlink -->
    <xsl:template match="xspf:link">
        <xsl:call-template name="handleHyperlink">
            <xsl:with-param name="string" select="."/>
            <xsl:with-param name="displayAs" select="'Link'"/>
        </xsl:call-template>
    </xsl:template>

    <!-- Format a single duration -->
    <xsl:template match="xspf:duration">
        <xsl:call-template name="formatDuration">
            <xsl:with-param name="milliseconds" select="."/>
        </xsl:call-template>
    </xsl:template>

</xsl:stylesheet>
