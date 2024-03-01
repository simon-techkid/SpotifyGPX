<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Pairs Listing</title>
                <link rel="stylesheet" href="styles.css" />
            </head>
            <body>
                <h1>Pairs Listing</h1>
                <hr />
                <ul>
                    <xsl:for-each select="Root/Line">
                        <li>
                            <a>
                                <xsl:attribute name="href">
                                    <xsl:value-of select="."/>
                                </xsl:attribute>
                                <xsl:value-of select="."/>
                            </a>
                        </li>
                    </xsl:for-each>
                </ul>
                <hr />
            </body>
        </html>
    </xsl:template>

</xsl:stylesheet>
