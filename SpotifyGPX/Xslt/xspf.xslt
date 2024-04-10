<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xspf="http://xspf.org/ns/0/" exclude-result-prefixes="xspf">
    
    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="name" select="xspf:playlist/xspf:title"/>
        <xsl:variable name="type" select="'Spotify Table'"/>
        <xsl:variable name="header" select="concat($type, ' - ', $name)"/>
        <xsl:variable name="stylesheet" select="'styles.css'"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
                <xsl:with-param name="stylesheet" select="$stylesheet"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="name" select="xspf:playlist/xspf:title"/>
                <xsl:with-param name="creator" select="xspf:playlist/xspf:creator"/>
                <xsl:with-param name="comment" select="xspf:playlist/xspf:annotation"/>
                <xsl:with-param name="hash" select="xspf:playlist/xspf:identifier"/>
                <xsl:with-param name="created" select="xspf:playlist/xspf:date"/>
                <xsl:with-param name="tracks" select="xspf:playlist/xspf:trackList/xspf:track"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the head section of the HTML -->
    <xsl:template name="html_head_template">
        <xsl:param name="title" />
        <xsl:param name="stylesheet" />
        <head>
            <title><xsl:value-of select="$title"/></title>
            <link rel="stylesheet" href="{$stylesheet}"/>
        </head>
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
            <xsl:call-template name="table">
                <xsl:with-param name="tracks" select="$tracks"/>
            </xsl:call-template>
            <hr />
        </body>
    </xsl:template>
    
    <!-- Template to create the table structure -->
    <xsl:template name="table">
        <xsl:param name="tracks"/>
        <table>
            <tr>
                <th>#</th>
                <th>Artist</th>
                <th>Album</th>
                <th>Title</th>
                <th>Time</th>
                <th>Link</th>
                <th>Played</th>
            </tr>
            <xsl:apply-templates select="$tracks"/>
            <tr>
                <td colspan="6">Total Duration</td>
                <td>
                    <xsl:call-template name="totalDuration">
                        <xsl:with-param name="tracks" select="$tracks"/>
                    </xsl:call-template>
                </td>
            </tr>
        </table>
    </xsl:template>

    <!-- Match each track and populate the table row -->
    <xsl:template match="xspf:track">
        <xsl:variable name="index" select="position()"/>
        <xsl:variable name="artist" select="xspf:creator"/>
        <xsl:variable name="album" select="xspf:album"/>
        <xsl:variable name="title" select="xspf:title"/>
        <xsl:variable name="time" select="xspf:annotation"/>
        <xsl:variable name="link">
            <xsl:call-template name="handleHyperlink">
                <xsl:with-param name="string" select="xspf:link"/>        
                <xsl:with-param name="displayAs" select="'Link'"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="played">
            <xsl:call-template name="formatDuration">
                <xsl:with-param name="milliseconds" select="xspf:duration"/>
            </xsl:call-template>    
        </xsl:variable>
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

    <!-- Template to handle hyperlink generation -->
    <xsl:template name="handleHyperlink">
        <xsl:param name="string" />
        <xsl:param name="displayAs" />
        <a href="{$string}"><xsl:value-of select="$displayAs"/></a>
    </xsl:template>

    <!-- Template to calculate total duration -->
    <xsl:template name="totalDuration">
        <xsl:param name="tracks" />
        <xsl:variable name="durations" select="$tracks/xspf:duration"/>
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
