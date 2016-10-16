<?xml version="1.0"?>

<!-- XSLT transformation to add styling to an GDAL/OGR-generated KML file -->
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:kml="http://www.opengis.net/kml/2.2"
  version="2.2">

  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" encoding="utf-8"/>

  <!-- Default is to allow all elements and attributes to pass through unchanged -->
  <xsl:template match="*">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <xsl:apply-templates  />
    </xsl:copy>
  </xsl:template>

  <!-- Remove any existing styles -->
  <xsl:template match="kml:Style" />

  <!-- Create new style for each placemark based on value of associated ABGRHexCode -->
  <xsl:template match="kml:Placemark">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <Style>
        <PolyStyle>
          <color>
            <xsl:value-of select="kml:ExtendedData/kml:SchemaData/kml:SimpleData[@name='ABGRHexCode']"/>
          </color>
          <fill>1</fill>
        </PolyStyle>
        <LineStyle>
          <color>ff000000</color>
        </LineStyle>
      </Style>
      <xsl:apply-templates />
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>