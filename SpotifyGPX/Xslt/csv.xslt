<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Template for the root element -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Pairs Table'"/>
        <xsl:variable name="header" select="$type"/>
        <xsl:variable name="stylesheet" select="'styles.css'"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
                <xsl:with-param name="stylesheet" select="$stylesheet"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="lines" select="//Line"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <xsl:template name="html_head_template">
        <xsl:param name="title"/>
        <xsl:param name="stylesheet"/>
        <head>
            <title><xsl:value-of select="$title"/></title>
            <link rel="stylesheet" href="{$stylesheet}"/>
        </head>
    </xsl:template>

    <xsl:template name="html_body_template">
        <xsl:param name="header" />
        <xsl:param name="lines" />
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr/>
            <xsl:call-template name="table">
                <xsl:with-param name="lines" select="$lines"/>
            </xsl:call-template>
            <hr/>
        </body>
    </xsl:template>

    <xsl:template name="table">
        <xsl:param name="lines" />
        <table>
            <!-- Apply template to process header line -->
            <tr>
                <xsl:call-template name="processLine">
                    <xsl:with-param name="line" select="$lines[1]"/>
                    <xsl:with-param name="isHeader" select="true()"/>
                </xsl:call-template>
            </tr>
            <!-- Apply templates to process non-header lines -->
            <xsl:apply-templates select="//Line[position() > 1]"/>
        </table>
    </xsl:template>

    <!-- Template to process each line -->
    <xsl:template match="Line">
        <tr>
            <!-- Call template to process the line -->
            <xsl:call-template name="processLine">
                <xsl:with-param name="line" select="."/>
                <xsl:with-param name="isHeader" select="false()"/>
            </xsl:call-template>
        </tr>
    </xsl:template>

    <!-- Template to split the contents of a line -->
    <xsl:template name="processLine">
        <xsl:param name="line"/>
        <xsl:param name="isHeader"/>
        <!-- Split the line by the delimiter -->
        <xsl:call-template name="split">
            <xsl:with-param name="string" select="$line"/>
            <xsl:with-param name="isHeader" select="$isHeader"/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to split a string by a delimiter -->
    <xsl:template name="split">
        <xsl:param name="string"/>
        <xsl:param name="isHeader"/>
        <xsl:choose>
            <!-- If the string contains quotes -->
            <xsl:when test="contains($string, '&quot;')">
                <xsl:variable name="value" select="substring-before(substring-after($string, '&quot;'), '&quot;')"/>
                <!-- If it's a header, create a th element, else create a td element -->
                <xsl:choose>
                    <xsl:when test="$isHeader">
                        <xsl:call-template name="header">
                            <xsl:with-param name="value" select="$value"/>
                        </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:call-template name="row">
                            <xsl:with-param name="value" select="$value"/>
                        </xsl:call-template>
                    </xsl:otherwise>
                </xsl:choose>
                <!-- Recursively call split template with the remaining string -->
                <xsl:call-template name="split">
                    <xsl:with-param name="string" select="substring-after(substring-after($string, '&quot;'), '&quot;')"/>
                    <xsl:with-param name="isHeader" select="$isHeader"/>
                </xsl:call-template>
            </xsl:when>
        </xsl:choose>
    </xsl:template>

    <!-- Template to process split values as table headers -->
    <xsl:template name="header">
        <xsl:param name="value"/>
        <th><xsl:value-of select="$value"/></th>
    </xsl:template>

    <!-- Template to process split values as table rows -->
    <xsl:template name="row">
        <xsl:param name="value"/>
        <td><xsl:value-of select="$value"/></td>
    </xsl:template>

</xsl:stylesheet>
