<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <!-- Template for the root element -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Pairs Table'"/>
        <xsl:variable name="header" select="$type"/>

        <xsl:call-template name="doctype"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$header"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$header"/>
                <xsl:with-param name="lines" select="//Line"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template for creating the HTML body -->
    <xsl:template name="html_body_template">
        <xsl:param name="header"/>
        <xsl:param name="lines"/>
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr/>
            <xsl:call-template name="table">
                <xsl:with-param name="lines" select="$lines"/>
            </xsl:call-template>
            <hr/>
        </body>
    </xsl:template>

    <!-- Template for creating the table and underlying rows -->
    <xsl:template name="table">
        <xsl:param name="lines"/>
        <table>
            <!-- Apply template to process header line -->
            <tr>
                <xsl:call-template name="processLine">
                    <xsl:with-param name="line" select="$lines[1]"/>
                    <xsl:with-param name="isHeader" select="true()"/>
                </xsl:call-template>
            </tr>
            <!-- Apply templates to process non-header lines -->
            <xsl:apply-templates select="$lines[position() > 1]"/>
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
    
    <xsl:template name="split">
        <xsl:param name="string" />
        <xsl:param name="isHeader"/>
        <xsl:choose>
            <!-- If the string contains a delimiter character -->
            <xsl:when test="contains($string, $delimiter)">
                <!-- Extract the value before the delimiter character -->
                <xsl:variable name="value" select="substring-before($string, $delimiter)" />

                <!-- Process the extracted value -->
                <xsl:call-template name="process-value">
                    <xsl:with-param name="isHeader" select="$isHeader"/>
                    <xsl:with-param name="value" select="$value" />
                </xsl:call-template>
                
                <!-- Recursively call split template with the remaining string -->
                <xsl:variable name="remaining" select="substring-after($string, $delimiter)" />
                <xsl:call-template name="split">
                    <xsl:with-param name="string" select="$remaining"/>
                    <xsl:with-param name="isHeader" select="$isHeader"/>
                </xsl:call-template>
            </xsl:when>
            <!-- If no delimiter character is found -->
            <xsl:otherwise>
                <!-- Process the remaining part of the string -->
                <xsl:call-template name="process-value">
                    <xsl:with-param name="isHeader" select="$isHeader"/>
                    <xsl:with-param name="value" select="$string" />
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>