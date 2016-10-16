/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 8 : Reprojection and Transformation
 */

-- Test scripts for transformation functions

DECLARE @Norwich geometry = geometry::STPointFromText('POINT(626000 354000)', 27700);
SELECT dbo.GeometryToGeography(@Norwich, 4326).ToString();
--53.035498 1.369338 (WGS84)

DECLARE @Norwich geometry = geometry::STPointFromText('POINT(626000 354000)', 27700);
SELECT dbo.GeometryToGeometry(@Norwich, 32631).ToString();
--390659,5877462 (UTM 31U)

DECLARE @Norwich geography = geography::STPointFromText('POINT(1.369338 53.035498)', 4326);
SELECT dbo.GeographyToGeometry(@Norwich, 32631).ToString();
--390659,5877462 (UTM 31U)

DECLARE @WGS84 geography = geography::STPointFromText('POINT(11.25 55.7765730186677)', 4326);
SELECT dbo.GeographyToGeometry(@WGS84, 3857).ToString();
-- 1252344.271424327 7514065.628545966 (Bing Maps)