<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <!-- Should encapsulating quotes be removed from table cells? -->
    <xsl:variable name="removeQuotes" select="true()"/>

    <!-- What does an encapsulating quote look like? -->
    <xsl:variable name="quotes" select="'&quot;'"/>

    <!-- Template to create table headers or table rows and remove encapsulating quotes -->
    <xsl:template name="process-value">
        <xsl:param name="value"/>
        <xsl:param name="isHeader"/>

        <xsl:variable name="string">
            <xsl:choose>
                <xsl:when test="$removeQuotes = true()">
                    <xsl:call-template name="removeQuotes">
                        <xsl:with-param name="string" select="$value"/>
                    </xsl:call-template>
                </xsl:when>
                <xsl:when test="$removeQuotes = false()">
                    <xsl:value-of select="$value"/>
                </xsl:when>
            </xsl:choose>
        </xsl:variable>

        <xsl:choose>
            <xsl:when test="$isHeader = true()">
                <th><xsl:value-of select="$string"/></th>
            </xsl:when>
            <xsl:when test="$isHeader = false()">
                <td><xsl:value-of select="$string"/></td>
            </xsl:when>
        </xsl:choose>
    </xsl:template>

    <!-- Template to remove encapsulating quotes from a given string -->
    <xsl:template name="removeQuotes">
        <xsl:param name="string"/>
        <xsl:variable name="startQuote" select="substring($string, 1, 1) = $quotes"/>
        <xsl:variable name="endQuote" select="substring($string, string-length($string), 1) = $quotes"/>
        <xsl:variable name="startRemoved">
            <xsl:choose>
                <xsl:when test="$startQuote">
                    <xsl:value-of select="substring($string, 2)"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$string"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="bothRemoved">
            <xsl:choose>
                <xsl:when test="$endQuote">
                    <xsl:value-of select="substring($startRemoved, 1, string-length($startRemoved) - 1)"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$startRemoved"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="$bothRemoved"/>
    </xsl:template>

</xsl:stylesheet>