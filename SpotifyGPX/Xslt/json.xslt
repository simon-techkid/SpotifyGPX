<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Spotify Table</title>
                <link rel="stylesheet" href="styles.css" />
            </head>
            <body>
                <h1>Spotify Table</h1>
                <hr />
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
                    <xsl:for-each select="Root/Root">
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
                    </xsl:for-each>
                    <tr>
                        <td colspan="7">Total Duration</td>
                        <td><xsl:call-template name="totalDuration"/></td>
                    </tr>
                </table>
                <hr />
            </body>
        </html>
    </xsl:template>

    <!-- Template to calculate total duration -->
    <xsl:template name="totalDuration">
        <xsl:variable name="totalMilliseconds" select="sum(Root/Root/msPlayed)"/>
        <xsl:call-template name="formatDuration">
            <xsl:with-param name="milliseconds" select="$totalMilliseconds"/>
        </xsl:call-template>
    </xsl:template>

    <xsl:template name="formatDuration">
        <!-- Input: Milliseconds (passed as a parameter) -->
        <xsl:param name="milliseconds" />

        <!-- Calculate hours, minutes, and seconds -->
        <xsl:variable name="hours" select="floor($milliseconds div 3600000)" />
        <xsl:variable name="minutes" select="floor(($milliseconds mod 3600000) div 60000)" />
        <xsl:variable name="seconds" select="floor(($milliseconds mod 60000) div 1000)" />

        <xsl:variable name="formattedH" select="format-number($hours, '00')" />
        <xsl:variable name="formattedM" select="format-number($minutes mod 60, '00')" />
        <xsl:variable name="formattedS" select="format-number($seconds mod 60, '00')" />

        <!-- Format the result -->
        <xsl:value-of select="concat($formattedH, ':', $formattedM, ':', $formattedS)"/>
    </xsl:template>

</xsl:stylesheet>
