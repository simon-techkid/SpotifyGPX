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
                        <th>Index</th>
                        <th>Track Name</th>
                        <th>Artist</th>
                        <th>Album</th>
                        <th>Duration</th>
                        <th>Reason Started</th>
                        <th>Reason Ended</th>
                    </tr>
                    <xsl:for-each select="Root/Root">
                        <tr>
                        <td><xsl:value-of select="Index"/></td>
                        <td><xsl:value-of select="master_metadata_track_name"/></td>
                        <td><xsl:value-of select="master_metadata_album_artist_name"/></td>
                        <td><xsl:value-of select="master_metadata_album_album_name"/></td>
                        <td><xsl:value-of select="TimePlayed"/></td>
                        <td><xsl:value-of select="reason_start"/></td>
                        <td><xsl:value-of select="reason_end"/></td>
                        </tr>
                    </xsl:for-each>
                </table>
            </body>
        </html>
    </xsl:template>

</xsl:stylesheet>
