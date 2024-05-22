<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:include href="extensions.xsl"/>
    <xsl:include href="tableextensions.xsl"/>
    <xsl:include href="separatedvaluestable.xsl"/>
    <xsl:output method="html" version="1.0" encoding="UTF-8" omit-xml-declaration="no" indent="yes" media-type="text/html"/>

    <!-- TSV column delimiter -->
    <xsl:variable name="delimiter" select="'&#9;'"/>

</xsl:stylesheet>
