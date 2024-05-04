<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:include href="extensions.xsl"/>
    <xsl:output method="html" version="1.0" encoding="UTF-8" omit-xml-declaration="no" indent="yes" media-type="text/html"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Song Table'"/>
        <xsl:variable name="header" select="$type"/>

        <xsl:call-template name="doctype"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="songs" select="Root/Object"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the body section of the HTML -->
    <xsl:template name="html_body_template">
        <xsl:param name="header"/>
        <xsl:param name="songs"/>
        <body>
                <h1><xsl:value-of select="$header"/></h1>
                <hr />
                <xsl:call-template name="table">
                    <xsl:with-param name="songs" select="$songs"/>
                </xsl:call-template>
                <hr />
            </body>
    </xsl:template>

    <!-- Template to create the table structure -->
    <xsl:template name="table">
        <xsl:param name="songs"/>
        <table>
            <tr>
                <th>#</th>
                <th>Description</th>
                <th>Index</th>
                <th>Source Time</th>
                <th>Used Time</th>
                <th>Artist</th>
                <th>Name</th>
            </tr>
            <xsl:apply-templates select="$songs"/>
        </table>
    </xsl:template>

    <!-- Match the Root/Root element and create a list item -->
    <xsl:template match="Root/Object">
        <tr>
            <td><xsl:value-of select="position()"/></td>
            <td><xsl:apply-templates select="Description"/></td>
            <td><xsl:value-of select="Index"/></td>
            <td><xsl:apply-templates select="CurrentInterpretation"/> at <xsl:value-of select="FriendlyTime"/></td>
            <td><xsl:apply-templates select="CurrentUsage"/> at <xsl:value-of select="Time"/></td>
            <td><xsl:value-of select="Song_Artist"/></td>
            <td><xsl:value-of select="Song_Name"/></td>
        </tr>
    </xsl:template>

    <xsl:template match="CurrentUsage">
        <xsl:choose>
            <xsl:when test=". = 0">Started</xsl:when>
            <xsl:when test=". = 1">Ended</xsl:when>
            <xsl:otherwise><xsl:value-of select="."/>Unknown</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="CurrentInterpretation">
        <xsl:choose>
            <xsl:when test=". = 0">Started</xsl:when>
            <xsl:when test=". = 1">Ended</xsl:when>
            <xsl:otherwise><xsl:value-of select="."/>Unknown</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!-- Template to match the <desc> tag -->
    <xsl:template match="Description">
        <xsl:call-template name="replace-newline">
            <xsl:with-param name="text" select="."/>
        </xsl:call-template>
    </xsl:template>

</xsl:stylesheet>
