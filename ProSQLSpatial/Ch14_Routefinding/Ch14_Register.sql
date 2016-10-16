/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 14 : Routefinding
 */
 
-- This script registers the SQLCLR procedures used in Ch14

DROP FUNCTION dbo.Extend;
DROP FUNCTION dbo.GeometryTSP;
DROP PROCEDURE dbo.GeographyAStar;

DROP ASSEMBLY Ch14_Routefinding;

CREATE ASSEMBLY Ch14_Routefinding
FROM 'C:\ProSQLSpatial\Ch14_Routefinding\bin\Debug\Ch14_Routefinding.dll'
WITH PERMISSION_SET = SAFE;
GO

CREATE FUNCTION dbo.Extend(@geom1 geometry, @geom2 geometry, @offset int) RETURNS geometry
EXTERNAL NAME Ch14_Routefinding.[ProSQLSpatial.Ch14.UserDefinedFunctions].Extend;
GO

CREATE FUNCTION dbo.GeometryTSP(@PlacesToVisit geometry) RETURNS geometry
EXTERNAL NAME Ch14_Routefinding.[ProSQLSpatial.Ch14.UserDefinedFunctions].GeometryTSP;
GO

CREATE PROCEDURE dbo.GeographyAStar(@StartID int, @GoalID int)
AS EXTERNAL NAME Ch14_Routefinding.[ProSQLSpatial.Ch14.StoredProcedures].GeographyAStar;
GO