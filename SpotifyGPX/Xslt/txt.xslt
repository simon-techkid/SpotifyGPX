<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <xsl:variable name="type" select="'Pairs Listing'"/>
        <xsl:variable name="stylesheet" select="'styles.css'"/>
        <html>
            <xsl:call-template name="html_head_template">
                <xsl:with-param name="title" select="$type"/>
                <xsl:with-param name="stylesheet" select="$stylesheet"/>
            </xsl:call-template>
            <xsl:call-template name="html_body_template">
                <xsl:with-param name="header" select="$type"/>
                <xsl:with-param name="lines" select="Root/Line"/>
            </xsl:call-template>
        </html>
    </xsl:template>

    <!-- Template to create the head section of the HTML -->
    <xsl:template name="html_head_template">
        <xsl:param name="title"/>
        <xsl:param name="stylesheet"/>
        <head>
            <title><xsl:value-of select="$title"/></title>
            <link rel="stylesheet" href="{$stylesheet}" />
        </head>
    </xsl:template>

    <!-- Template to create the body section of the HTML -->
    <xsl:template name="html_body_template">
        <xsl:param name="header"/>
        <xsl:param name="lines"/>
        <body>
            <h1><xsl:value-of select="$header"/></h1>
            <hr />
            <xsl:call-template name="bulletedList">
                <xsl:with-param name="lines" select="$lines"/>
            </xsl:call-template>
            <hr />
        </body>
    </xsl:template>

    <!-- Template to create a bulleted list -->
    <xsl:template name="bulletedList">
        <xsl:param name="lines"/>
        <ul>
            <xsl:apply-templates select="$lines"/>
        </ul>
    </xsl:template>

    <!-- Template to create a numbered list -->
    <xsl:template name="numberedList">
        <xsl:param name="lines"/>
        <ol>
            <xsl:apply-templates select="$lines"/>
        </ol>
    </xsl:template>

    <!-- Match the Line element and create a list item -->
    <xsl:template match="Root/Line">
        <li>
            <xsl:call-template name="handleHyperlink">
                <xsl:with-param name="string" select="."/>
                <xsl:with-param name="displayAs" select="."/>
            </xsl:call-template>
        </li>
    </xsl:template>

    <!-- Template to handle hyperlink generation -->
    <xsl:template name="handleHyperlink">
        <xsl:param name="string"/>
        <xsl:param name="displayAs"/>
        <a href="{$string}"><xsl:value-of select="$displayAs"/></a>
    </xsl:template>

</xsl:stylesheet>
