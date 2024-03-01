<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Pairs Table</title>
                <link rel="stylesheet" href="styles.css"/>
            </head>
            <body>
                <h1>Pairs Table</h1>
                <hr/>
                <table>
                    <tr>
                        <xsl:call-template name="split">
                            <xsl:with-param name="string" select="normalize-space(//Line[1])"/>
                            <xsl:with-param name="header" select="true()"/>
                        </xsl:call-template>
                    </tr>
                    <xsl:apply-templates select="//Line[position() > 1]"/>
                </table>
                <hr/>
            </body>
        </html>
    </xsl:template>

    <!-- Split the contents of each line (non header) of the CSV file -->
    <xsl:template match="Line">
        <tr>
            <xsl:call-template name="split">
                <xsl:with-param name="string" select="normalize-space(.)"/>
                <xsl:with-param name="header" select="false()"/>
            </xsl:call-template>
        </tr>
    </xsl:template>

    <!-- Split the contents of a string by a delimiter into multiple cells -->
    <xsl:template name="split">
        <xsl:param name="string"/>
        <xsl:param name="header"/>
        <xsl:param name="delimiter" select="','"/>
        <xsl:choose>
            <xsl:when test="contains($string, '&quot;')">
                <xsl:variable name="value" select="substring-before(substring-after($string, '&quot;'), '&quot;')"/>
                <xsl:choose>
                    <xsl:when test="$header">
                        <th><xsl:value-of select="$value"/></th>
                    </xsl:when>
                    <xsl:otherwise>
                        <td><xsl:value-of select="$value"/></td>
                    </xsl:otherwise>
                </xsl:choose>
                <xsl:call-template name="split">
                    <xsl:with-param name="string" select="substring-after(substring-after($string, '&quot;'), '&quot;')"/>
                    <xsl:with-param name="header" select="$header"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:choose>
                    <xsl:when test="$header">
                        <th><xsl:value-of select="$string"/></th>
                    </xsl:when>
                    <xsl:otherwise>
                        <td><xsl:value-of select="$string"/></td>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>
