DROP FUNCTION dbo.GeographySetZ;
DROP ASSEMBLY Ch16_Visualisation;

/**
 * REGISTER ASSEMBLY AND FUNCTIONS
 */
CREATE ASSEMBLY Ch16_Visualisation
FROM 'C:\Users\Alastair\Documents\Visual Studio 2008\Projects\ProSQLSpatial\Ch16_VisualisationUI_GoogleEarth\bin\Debug\Ch16_VisualisationUI_GoogleEarth.dll'
WITH PERMISSION_SET = SAFE;
GO

CREATE FUNCTION dbo.GeographySetZ(@Geog geography, @Z float) RETURNS geography
AS EXTERNAL NAME Ch16_Visualisation.[ProSQLSpatial.Ch16_Visualisation].SetGeographyZ;
GO