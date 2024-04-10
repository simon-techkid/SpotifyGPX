<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Spotify Table'"/>
        <xsl:variable name="header" select="$type"/>
        <xsl:variable name="stylesheet" select="'styles.css'"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
                <xsl:with-param name="stylesheet" select="$stylesheet"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="songs" select="Root/Root"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the head section of the HTML -->
    <xsl:template name="html_head_template">
        <xsl:param name="title" />
        <xsl:param name="stylesheet" />
        <head>
            <title><xsl:value-of select="$title"/></title>
            <link rel="stylesheet" href="{$stylesheet}" />
        </head>
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
                <th>Artist</th>
                <th>Album</th>
                <th>Title</th>
                <th>Time</th>
                <th>Country</th>
                <th>IP Address</th>
                <th>Duration</th>
            </tr>
            <xsl:apply-templates select="$songs"/>
            <tr>
                <td colspan="7">Total Duration</td>
                <td>
                    <xsl:call-template name="totalDuration">
                        <xsl:with-param name="songs" select="$songs"/>
                    </xsl:call-template>
                </td>
            </tr>
        </table>
    </xsl:template>

    <!-- Match the Root/Root element and create a list item -->
    <xsl:template match="Root/Root">
        <tr>
            <td><xsl:value-of select="position()"/></td>
            <td><xsl:value-of select="master_metadata_album_artist_name"/></td>
            <td><xsl:value-of select="master_metadata_album_album_name"/></td>
            <td><xsl:value-of select="master_metadata_track_name"/></td>
            <td><xsl:value-of select="ts"/></td>
            <td><xsl:value-of select="conn_country"/></td>
            <td><xsl:value-of select="ip_addr_decrypted"/></td>
            <td>
                <xsl:call-template name="formatDuration">
                    <xsl:with-param name="milliseconds" select="msPlayed"/>
                </xsl:call-template>
            </td>
        </tr>
    </xsl:template>

    <!-- Template to calculate total duration -->
    <xsl:template name="totalDuration">
        <xsl:param name="songs"/>
        <xsl:variable name="durations" select="$songs/msPlayed"/>
        <xsl:variable name="totalMilliseconds" select="sum($durations)"/>
        <xsl:call-template name="formatDuration">
            <xsl:with-param name="milliseconds" select="$totalMilliseconds"/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to format milliseconds into HH:MM:SS -->
    <xsl:template name="formatDuration">
        <xsl:param name="milliseconds" />
        <xsl:variable name="hours" select="floor($milliseconds div 3600000)" />
        <xsl:variable name="minutes" select="floor(($milliseconds mod 3600000) div 60000)" />
        <xsl:variable name="seconds" select="floor(($milliseconds mod 60000) div 1000)" />
        <xsl:variable name="formattedH" select="format-number($hours, '00')" />
        <xsl:variable name="formattedM" select="format-number($minutes mod 60, '00')" />
        <xsl:variable name="formattedS" select="format-number($seconds mod 60, '00')" />
        <xsl:value-of select="concat($formattedH, ':', $formattedM, ':', $formattedS)"/>
    </xsl:template>

</xsl:stylesheet>
