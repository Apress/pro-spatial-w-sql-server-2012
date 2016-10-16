/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 8 : Reprojection and Transformation
 */

-- Registration scripts for transformation functions

DROP FUNCTION dbo.GeometryToGeometry
DROP FUNCTION dbo.GeometryToGeography
DROP FUNCTION dbo.GeographyToGeometry
DROP FUNCTION dbo.GeographyToGeography

DROP ASSEMBLY Ch8_Transformation

CREATE ASSEMBLY Ch8_Transformation
FROM 'C:\ProSQLSpatial\Ch8_Transformation\bin\Debug\Ch8_Transformation.dll'
WITH PERMISSION_SET = SAFE;
GO

CREATE FUNCTION dbo.GeometryToGeometry(@geom geometry, @srid int) RETURNS geometry
EXTERNAL NAME Ch8_Transformation.[Ch8_Transformation.UserDefinedFunctions].GeometryToGeometry
GO

CREATE FUNCTION dbo.GeographyToGeometry(@geog geography, @srid int) RETURNS geometry
EXTERNAL NAME Ch8_Transformation.[Ch8_Transformation.UserDefinedFunctions].GeographyToGeometry
GO

CREATE FUNCTION dbo.GeometryToGeography(@geom geometry, @srid int) RETURNS geography
EXTERNAL NAME Ch8_Transformation.[Ch8_Transformation.UserDefinedFunctions].GeometryToGeography
GO

CREATE FUNCTION dbo.GeographyToGeography(@geog geography, @srid int) RETURNS geography
EXTERNAL NAME Ch8_Transformation.[Ch8_Transformation.UserDefinedFunctions].GeographyToGeography
GO
