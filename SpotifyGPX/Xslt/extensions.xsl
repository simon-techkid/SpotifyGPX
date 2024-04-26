<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <!-- New line character for description line breaks -->
    <xsl:variable name="newLine" select="'&#xA;'"/>
    <xsl:variable name="stylesheet" select="'styles.css'"/>

    <!-- Template to create the doctype -->
    <xsl:template name="doctype">
        <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html&gt;</xsl:text>
        <xsl:value-of select="$newLine"/>
    </xsl:template>

    <!-- Template to create the head section of the HTML -->
    <xsl:template name="html_head_template">
        <xsl:param name="title" />
        <head>
            <title><xsl:value-of select="$title"/></title>
            <link rel="stylesheet" href="{$stylesheet}" />
        </head>
    </xsl:template>

    <!-- Template to recursively replace CRLF with <br> -->
    <xsl:template name="replace-newline">
        <xsl:param name="text"/>
        <xsl:choose>
            <xsl:when test="contains($text, $newLine)">
                <xsl:value-of select="substring-before($text, $newLine)"/>
                <br/>
                <xsl:call-template name="replace-newline">
                    <xsl:with-param name="text" select="substring-after($text, $newLine)"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$text"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!-- Template to handle hyperlink generation -->
    <xsl:template name="handleHyperlink">
        <xsl:param name="string"/>
        <xsl:param name="displayAs"/>
        <a href="{$string}"><xsl:value-of select="$displayAs"/></a>
    </xsl:template>

    <!-- Template to calculate total duration -->
    <xsl:template name="totalDuration">
        <xsl:param name="durations" />
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