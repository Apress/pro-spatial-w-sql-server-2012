/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 10 : Modification
 */

-- Simplifying a geometry
DECLARE @LineString geometry;
SET @LineString = 'LINESTRING(130 33, 131 33.5, 131.5 32.9, 133 32.5, 135 33, 137 32, 138 31, 140 30)';
SELECT @LineString.Reduce(1).ToString() AS SimplifiedLine;
GO

-- Creating a linear approximation of a curved geometry
DECLARE @CircularString geometry;
SET @CircularString = 'CIRCULARSTRING(10 0, 0 10, -10 0, 0 -10, 10 0)'; 
DECLARE @LineString geometry;
SET @LineString = @CircularString.STCurveToLine();
SELECT
  @CircularString.STLength(),
  @LineString.STLength(),
  @CircularString.STNumPoints(),
  @LineString.STNumPoints();

-- Reversing the ring orientation of a polygon
DECLARE @polygon geography = 'POLYGON((-2 50, 4 52, -1 60, -2 50))';
SELECT @polygon.ReorientObject();
GO

-- More verbose way of reversing the ring orientation of a polygon
DECLARE @polygon geography = 'POLYGON((-2 50, 4 52, -1 60, -2 50))';
DECLARE @world geography = geography::STGeomFromText('FULLGLOBE', @polygon.STSrid);
SELECT @world.STDifference(@polygon);

-- Creating a buffer around a geometry
DECLARE @Restaurant geography;
SET @Restaurant = geography::STGeomFromText('POINT(1.3033 52.6285)', 4326);
DECLARE @FreeDeliveryZone geography;
SET @FreeDeliveryZone = @Restaurant.STBuffer(5000);
SELECT 
  @FreeDeliveryZone,
  @FreeDeliveryZone.STAsText() AS WKT;
GO

-- Creating a simpler buffer
DECLARE @Restaurant geography;
SET @Restaurant = geography::STGeomFromText('POINT(1.3033 52.6285)', 4326);
DECLARE @FreeDeliveryZone geography;
SET @FreeDeliveryZone = @Restaurant.BufferWithTolerance(5000, 250, 'false');
SELECT
  @FreeDeliveryZone,
  @FreeDeliveryZone.STAsText() AS WKT;
GO

-- Creating a circular buffer
DECLARE @Restaurant geography;
SET @Restaurant = geography::STGeomFromText('POINT(1.3033 52.6285)', 4326);
DECLARE @FreeDeliveryZone geography;
SET @FreeDeliveryZone = @Restaurant.BufferWithCurves(5000);
SELECT
  @FreeDeliveryZone,
  @FreeDeliveryZone.STAsText() AS WKT;
GO

-- Creating the convex hull of a geometry
DECLARE @H5N1 geography;
SET @H5N1 = geography::STMPointFromText(
  'MULTIPOINT(
    105.968 20.541, 105.877 21.124, 106.208 20.28, 101.803 16.009, 99.688 16.015,
    99.055 14.593, 99.055 14.583, 102.519 16.215, 100.914 15.074, 102.117 14.957,
    100.527 14.341, 99.699 17.248, 99.898 14.608, 99.898 14.608, 99.898 14.608,
    99.898 14.608, 100.524 17.75, 106.107 21.11, 106.91 11.753, 107.182 11.051,
    105.646 20.957, 105.857 21.124, 105.867 21.124, 105.827 21.124, 105.847 21.144,
    105.847 21.134, 106.617 10.871, 106.617 10.851, 106.637 10.851, 106.617 10.861,
    106.627 10.851, 106.617 10.881, 108.094 11.77, 108.094 11.75, 108.081 11.505,
    108.094 11.76, 105.899 9.546, 106.162 11.414, 106.382 20.534, 106.352 20.504, 
    106.342 20.504, 106.382 20.524, 106.382 20.504, 105.34 20.041, 105.34 20.051, 
    104.977 22.765, 105.646 20.977, 105.646 20.937, 99.688 16.015, 100.389 13.927,
    101.147 16.269, 101.78 13.905, 99.704 17.601, 105.604 10.654, 105.817 21.124, 
    106.162 11.404, 106.362 20.504)',
  4326);
SELECT 
  @H5N1 AS Shape
UNION ALL SELECT
  @H5N1.STConvexHull() AS Shape;

