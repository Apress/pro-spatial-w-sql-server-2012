/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 15 : Tesselation and Triangulation
 * 
 * Import the SQLCLR assembly and register the functions used in this chapter
 */

/**
 * DROP ANY EXISTING VERSIONS
 */
DROP PROCEDURE dbo.GeometryTriangulate;
DROP PROCEDURE dbo.GeometryTriangulate3d;
DROP FUNCTION dbo.GeometryAlphaShape;
DROP PROCEDURE dbo.GeometryVoronoi;
DROP ASSEMBLY Ch15_Triangulation;

/**
 * REGISTER ASSEMBLY AND FUNCTIONS
 */
CREATE ASSEMBLY Ch15_Triangulation
FROM 'C:\Users\Alastair\Documents\Visual Studio 2008\Projects\ProSQLSpatial\Ch15_Triangulation\bin\Debug\Ch15_Triangulation.dll'
WITH PERMISSION_SET = SAFE;
GO
CREATE PROCEDURE dbo.GeometryTriangulate(@MultiPoint geometry)
AS EXTERNAL NAME Ch15_Triangulation.[ProSQLSpatial.Ch15_Triangulation].GeometryTriangulate;
GO
CREATE PROCEDURE dbo.GeometryTriangulate3d(@MultiPoint geometry)
AS EXTERNAL NAME Ch15_Triangulation.[ProSQLSpatial.Ch15_Triangulation].GeometryTriangulate3d;
GO
CREATE FUNCTION dbo.GeometryAlphaShape(@MultiPoint geometry, @alpha float) RETURNS geometry
AS EXTERNAL NAME Ch15_Triangulation.[ProSQLSpatial.Ch15_Triangulation].GeometryAlphaShape;
GO
CREATE PROCEDURE dbo.GeometryVoronoi(@MultiPoint geometry)
AS EXTERNAL NAME Ch15_Triangulation.[ProSQLSpatial.Ch15_Triangulation].GeometryVoronoi;
GO