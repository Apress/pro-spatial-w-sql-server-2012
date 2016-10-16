/**
 * Pro Spatial with SQL Server 2012
 * Chapter 13 : Clustering and Distribution Analysis
 * Alastair Aitchison, 2012
 */ 

-- DROP any old versions of objects created by this script
/*
DROP PROCEDURE dbo.GeometrykMeans;
DROP PROCEDURE dbo.CreateGeometryGrid;
DROP PROCEDURE dbo.CreateGeometryDartboard;
DROP ASSEMBLY ProSQLSpatial_Ch13_Clustering;
*/

-- Register the assembly
CREATE ASSEMBLY ProSpatial_Ch13_Clustering
FROM 'C:\ProSpatial\Ch13_Clustering\bin\Debug\ProSQLSpatial_Ch13_Clustering.dll'
WITH PERMISSION_SET = SAFE;
GO

CREATE PROCEDURE dbo.GeometrykMeans(
  @multipoint geometry,
  @k int)
AS EXTERNAL NAME ProSpatial_Ch13_Clustering.[ProSpatial.Ch13.StoredProcedures].GeometrykMeans;
GO

CREATE PROCEDURE dbo.CreateGeometryGrid (
   @boundingbox geometry,
   @columns int,
   @rows int)
AS EXTERNAL NAME ProSpatial_Ch13_Clustering.[ProSpatial.Ch13.StoredProcedures].CreateGeometryGrid;
GO

CREATE PROCEDURE dbo.CreateGeometryDartboard (
   @centre geometry,
   @radius float,
   @numrings int)
AS EXTERNAL NAME ProSpatial_Ch13_Clustering.[ProSpatial.Ch13.StoredProcedures].CreateGeometryDartboard;
GO