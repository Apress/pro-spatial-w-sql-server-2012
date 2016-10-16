/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 2 : Spatial Features
 */

-- Select a geometry Point at x=5, y=3, using SRID 0
SELECT geometry::STPointFromText('POINT (5 3)', 0);

-- Select a geography Point at longitude=60 degrees, latitude=40 degrees, using SRID 4326
SELECT geography::STPointFromText('POINT (60 40)', 4326);

-- Select 4d geometry Point at x=5, y=3, z=1, m=10.2, using SRID 0
SELECT geometry::STPointFromText('POINT(5 3 1 10.2)', 0);

-- Create a geometry LineString
SELECT geometry::STLineFromText('LINESTRING(2 3, 4 6, 6 6, 10 4)', 0);

-- Create a geometry CircularString arc
SELECT geometry::STGeomFromText('CIRCULARSTRING(1 3, 4 1, 9 4)', 0);

-- Creating straight CircularStrings
DECLARE @LineString geometry = 'LINESTRING(0 0, 8 6)';
DECLARE @CircularString1 geometry = 'CIRCULARSTRING(0 0, 4 3, 8 6)';
DECLARE @CircularString2 geometry = 'CIRCULARSTRING(0 0, 0 0, 8 6)';
SELECT
  @LineString AS LineString,
  @CircularString1 AS CircularString1,
  @CircularString2 AS CircularString2,
  @LineString.STEquals(@CircularString1), -- Returns 1 (true)
  @LineString.STEquals(@CircularString2);  -- Returns 1 (true)

-- Converting from CircularString to LineString
DECLARE @CircularString geometry = geometry::STGeomFromText('CIRCULARSTRING(1 3, 4 1, 9 4)', 0);
SELECT @CircularString.STCurveToLine();

-- Creating a geometry CompoundCurve
SELECT geometry::STGeomFromText('COMPOUNDCURVE((2 3, 2 8),CIRCULARSTRING(2 8, 4 10, 6 8),(6 8, 6 3),CIRCULARSTRING(6 3, 4 1, 2 3))', 0);

-- Creating a rectangular Polygon
SELECT geometry::STPolyFromText('POLYGON((1 1, 3 1, 3 7, 1 7, 1 1))', 0);

-- Creating a triangular Polygon containing an interior hole
SELECT geometry::STPolyFromText('POLYGON((10 1, 10 9, 4 9, 10 1), (9 4, 9 8, 6 8, 9 4))', 0);

-- Creating a (circular) CurvePolygon
SELECT geometry::STGeomFromText('CURVEPOLYGON(CIRCULARSTRING(4 2, 8 2, 8 6, 4 6, 4 2))', 0);

-- Creating a MultiPoint containing three points
SELECT geometry::STMPointFromText('MULTIPOINT(0 0, 2 4, 10 8)', 0);

-- Creating a MultiPoint containing two points, each having X/Y/Z coordinates
SELECT geometry::STMPointFromText('MULTIPOINT(0 0 2, 4 10 8)', 0);

-- Creating a MultiLineString
SELECT geometry::STMLineFromText('MULTILINESTRING((0 0, 2 2), (3 2, 6 9), (3 3, 5 3, 8 8))', 0);

-- Creating a MultiPolygon
SELECT geometry::STMPolyFromText('MULTIPOLYGON(((10 20, 30 10, 44 50, 10 20)), ((35 36, 37 37, 38 34, 35 36)))', 0);

-- Creating a Geometry Collection
SELECT geometry::STGeomCollFromText('GEOMETRYCOLLECTION(POLYGON((5 5, 10 5, 10 10, 5 5)), POINT(10 12))', 0);

-- Creating a FullGlobe using SRID 4326
SELECT geography::STGeomFromText('FULLGLOBE', 4326);

-- Creating an empty Point geometry using SRID 4269
SELECT geography::STGeomFromText('POINT EMPTY', 4269);