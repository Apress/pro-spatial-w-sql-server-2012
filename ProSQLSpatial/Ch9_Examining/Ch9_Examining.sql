/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 9 : Examining Spatial Properties
 */

 -- Retrieve the type of a geometry
DECLARE @Line geometry;
SET @Line = 'LINESTRING(0 0, 5 2, 8 3)';
SELECT @Line.STGeometryType();

-- Using InstanceOf() to assess a geometry type
DECLARE @CircularString geometry;
SET @CircularString = 'CIRCULARSTRING(0 0, 3 5, 6 1)';
SELECT
  @CircularString.InstanceOf('Curve'),           -- 1
  @CircularString.InstanceOf('CircularString'),  -- 1
  @CircularString.InstanceOf('LineString');      -- 0

-- Testing for simplicity
DECLARE @DeliveryRoute geometry;
SET @DeliveryRoute = geometry::STLineFromText(
  'LINESTRING(586960 4512940, 586530 4512160, 585990 4512460,
  586325 4513096, 587402 4512517, 587480 4512661)', 32618);
SELECT 
  @DeliveryRoute AS Shape,
  @DeliveryRoute.STIsSimple() AS IsSimple;

-- Rearranging the point order makes the route simple
DECLARE @DeliveryRoute geometry;
SET @DeliveryRoute = geometry::STLineFromText(
  'LINESTRING(586960 4512940, 587480 4512661, 587402 4512517,
  586325 4513096, 585990 4512460, 586530 4512160)', 32618);
SELECT 
  @DeliveryRoute AS Shape,
  @DeliveryRoute.STIsSimple() AS IsSimple;

-- Testing if a geometry is closed
DECLARE @Snowdon geography;
SET @Snowdon = geography::STMLineFromText(
'MULTILINESTRING(
 (-4.07668 53.06804 3445,  -4.07694 53.06832 3445,  -4.07681 53.06860 3445,
  -4.07668 53.06869 3445,  -4.07651 53.06860 3445,  -4.07625 53.06832 3445,
  -4.07661 53.06804 3445,  -4.07668 53.06804 3445),
 (-4.07668 53.06776 3412,  -4.07709 53.06795 3412,  -4.07717 53.06804 3412,
  -4.07730 53.06832 3412,  -4.07730 53.06860 3412,  -4.07709 53.06890 3412,
  -4.07668 53.06898 3412,  -4.07642 53.06890 3412,  -4.07597 53.06860 3412,
  -4.07582 53.06832 3412,  -4.07603 53.06804 3412,  -4.07625 53.06791 3412,
  -4.07668 53.06776 3412),
 (-4.07709 53.06768 3379,  -4.07728 53.06778 3379,  -4.07752 53.06804 3379,
  -4.07767 53.06832 3379,  -4.07773 53.06860 3379,  -4.07771 53.06890 3379,
  -4.07728 53.06918 3379,  -4.07657 53.06918 3379,  -4.07597 53.06890 3379,
  -4.07582 53.06879 3379,  -4.07541 53.06864 3379,  -4.07537 53.06860 3379,
  -4.07526 53.06832 3379,  -4.07556 53.06804 3379,  -4.07582 53.06795 3379,
  -4.07625 53.06772 3379,  -4.07668 53.06757 3379,  -4.07709 53.06768 3379))',
  4326);
SELECT
  @Snowdon AS Shape,
  @Snowdon.STIsClosed() AS IsClosed;

-- Testing if a geometry is a ring
DECLARE @Speedway geometry;
SET @Speedway = geometry::STLineFromText(
   'LINESTRING(565900 4404737, 565875 4405861, 565800 4405987, 565670 4406055,
     565361 4406050, 565222 4405975, 565150 4405825, 565170 4404760, 565222 4404617,
     565361 4404521, 565700 4404524, 565834 4404603, 565900 4404737)', 32616);
SELECT 
  @Speedway AS Shape,
  @Speedway.STIsRing() AS IsRing;

-- Counting the number of points in a geometry - note one more than you may expect for a Polygon
DECLARE @BermudaTriangle geography;
SET @BermudaTriangle = geography::STPolyFromText(
  'POLYGON((-66.07 18.45, -64.78 32.3, -80.21 25.78, -66.07 18.45))',
  4326);
SELECT
  @BermudaTriangle AS Shape,
  @BermudaTriangle.STNumPoints() AS NumPoints;

-- Testing if a geometry is empty
DECLARE @LineString1 geometry;
DECLARE @LineString2 geometry;
SET @LineString1 = geometry::STLineFromText('LINESTRING(2 4, 10 6)', 0);
SET @LineString2 = geometry::STLineFromText('LINESTRING(0 2, 8 4)', 0);
SELECT 
  @LineString1.STUnion(@LineString2) AS Shape,
  @LineString1.STIntersection(@LineString2).STIsEmpty() AS IsEmpty;

-- Returning a specific point by index
DECLARE @LondonMarathon geography;
SET @LondonMarathon = geography::STMPointFromText(
  'MULTIPOINT(0.0112 51.4731, 0.0335 51.4749, 0.0527 51.4803, 0.0621 51.4906,
  0.0448 51.4923, 0.0238 51.4870, 0.0021 51.4843, -0.0151 51.4814,
  -0.0351 51.4861, -0.0460 51.4962, -0.0355 51.5011, -0.0509 51.5013,
  -0.0704 51.4989, -0.0719 51.5084, -0.0493 51.5098, -0.0275 51.5093,
  -0.0257 51.4963, -0.0134 51.4884, -0.0178 51.5003, -0.0195 51.5046,
  -0.0087 51.5072, -0.0278 51.5112, -0.0472 51.5099, -0.0699 51.5084,
  -0.0911 51.5105, -0.1138 51.5108, -0.1263 51.5010, -0.1376 51.5031)',
  4326);
SELECT
  @LondonMarathon AS Shape,
  @LondonMarathon.STPointN(14) AS Point14,
  @LondonMarathon.STPointN(14).STAsText() AS WKT;

-- Returning the start/end point of a geometry
DECLARE @TransatlanticCrossing geography;
SET @TransatlanticCrossing = geography::STLineFromText('
LINESTRING( 
  -73.88 40.57, -63.57 44.65, -53.36 46.74, -28.63 38.54,
  -28.24 38.42, -9.14 38.71,  -8.22 43.49,  -4.14 50.37)',
  4326
);
SELECT
  @TransatlanticCrossing AS Shape,
  @TransatlanticCrossing.STStartPoint().STAsText() AS StartPoint,
  @TransatlanticCrossing.STEndPoint().STAsText() AS EndPoint; 

-- Calculating the centroid of a geometry instance
DECLARE @Colorado geometry;
SET @Colorado = geometry::STGeomFromText('POLYGON((-102.0423 36.9931, -102.0518 
41.0025, -109.0501 41.0006, -109.0452 36.9990, -102.0423 36.9931))', 4326);
SELECT 
  @Colorado AS Shape,
  @Colorado.STCentroid() AS Centroid,
  @Colorado.STCentroid().STAsText() AS WKT;

-- Calculating the centre of the envelope around a geography instance
DECLARE @Utah geography;
SET @Utah = geography::STPolyFromText(
  'POLYGON((-109 37, -109 41, -111 41, -111 42, -114 42, -114 37, -109 37))', 4326);
SELECT 
  @Utah AS Shape,
  @Utah.EnvelopeCenter() AS EnvelopeCenter,
  @Utah.EnvelopeCenter().STAsText() AS WKT;

-- Returning an arbitrary point from a geometry
DECLARE @Polygon geometry;
SET @Polygon = geometry::STGeomFromText('POLYGON((10 2,10 4,5 4,5 2,10 2))',0);
SELECT
  @Polygon AS Shape,
  @Polygon.STPointOnSurface() AS PointOnSurface,
  @Polygon.STPointOnSurface().STAsText() AS WKT;

-- Returning individual X/Y coordinates from a geometry Point
DECLARE @Johannesburg geometry;
SET @Johannesburg = geometry::STGeomFromText('POINT(604931 7107923)', 32735);
SELECT
  @Johannesburg.STX AS X, 
  @Johannesburg.STY AS Y;

-- Returning individual Lat/Long coordinates from a geography Point
DECLARE @Colombo geography;
SET @Colombo =
  geography::STGeomFromWKB(0x01010000006666666666F65340B81E85EB51B81B40, 4326);
SELECT
  @Colombo.Long AS Longitude,
  @Colombo.Lat AS Latitude;

-- Returning 3- and 4- dimensional coordinates
DECLARE @Antenna geography;
SET @Antenna = 
  geography::STPointFromText('POINT(-89.64778 39.83167 34.7 1000131)', 4269);
SELECT
  @Antenna.HasM AS HasM,
  @Antenna.M AS M,
  @Antenna.HasZ AS HasZ,
  @Antenna.Z AS Z;

-- Calculate the boundary of a geometry
DECLARE @A geometry;
SET @A = geometry::STPolyFromText(
  'POLYGON((0 0, 4 0, 6 5, 14 5, 16 0, 20 0, 13 20, 7 20, 0 0),
    (7 8,13 8,10 16,7 8))', 0);
SELECT
  @A AS Shape,
  @A.STBoundary() AS Boundary,
  @A.STBoundary().STAsText() AS WKT;

-- Calculate the envelope of a geometry
DECLARE @A geometry;
SET @A = geometry::STPolyFromText(
  'POLYGON((0 0, 4 0, 6 5, 14 5, 16 0, 20 0, 13 20, 7 20, 0 0),
  (7 8,13 8,10 16,7 8))', 0);
SELECT
  @A AS Shape,
  @A.STEnvelope() AS Envelope,
  @A.STEnvelope().STAsText() AS WKT;

-- Determining the extent of a geography
DECLARE @NorthernHemisphere geography
SET @NorthernHemisphere = 
  geography::STGeomFromText('POLYGON((0 0.1,90 0.1,180 0.1, -90 0.1, 0 0.1))',4326)
SELECT
  @NorthernHemisphere AS Shape,
  @NorthernHemisphere.EnvelopeAngle() AS EnvelopeAngle;

-- Creating the "envelope" of a geography instance
DECLARE @geog geography;
SET @geog = geography::STPolyFromText('POLYGON((-4 50, 2 52, -1 60, -4 50))', 4326);
DECLARE @geom geometry;
SET @geom = geometry::STPolyFromWKB(@geog.STAsBinary(), @geog.STSrid);
DECLARE @geomboundingbox geometry;
SET @geomboundingbox = @geom.STEnvelope();
DECLARE @geogboundingbox geography;
SET @geogboundingbox = geography::STPolyFromWKB(@geomboundingbox.STAsBinary(), @geomboundingbox.STSrid);
SELECT @geogboundingbox.ToString();
GO

-- Isolating the exterior ring of a geometry
DECLARE @A geometry;
SET @A = geometry::STPolyFromText(
  'POLYGON((0 0, 4 0, 6 5, 14 5, 16 0, 20 0, 13 20, 7 20, 0 0),
            (7 8,13 8,10 16,7 8))', 0);
SELECT 
  @A AS Shape,
  @A.STExteriorRing() AS ExteriorRing,
  @A.STExteriorRing().STAsText() AS WKT;

-- Counting the number of interior rings
DECLARE @Polygon geometry;
SET @Polygon = geometry::STPolyFromText('
  POLYGON(
    (0 0, 20 0, 20 10, 0 10, 0 0),
    (3 1,3 8,2 8,3 1),
    (14 2,18 6, 12 4, 14 2))',
    0);
SELECT
  @Polygon AS Shape,
  @Polygon.STNumInteriorRing() AS NumInteriorRing;
GO

-- Isolating an individual ring
DECLARE @A geometry;
SET @A = geometry::STPolyFromText(
  'POLYGON((0 0, 4 0, 6 5, 14 5, 16 0, 20 0, 13 20, 7 20, 0 0),
    (7 8,13 8,10 16,7 8))', 0);
SELECT 
  @A AS Shape,
  @A.STInteriorRingN(1) AS InteriorRing1,
  @A.STInteriorRingN(1).STAsText() AS WKT;

-- Counting rings in a geography instance
DECLARE @Pentagon geography;
SET @Pentagon = geography::STPolyFromText(
  'POLYGON(
    (
      -77.0532 38.87086,
      -77.0546 38.87304,
      -77.0579 38.87280,
      -77.0585 38.87022,
      -77.0555 38.86907,
      -77.0532 38.87086 
    ),
    (
      -77.0558 38.87028,
      -77.0569 38.87073,
      -77.0567 38.87170,
      -77.0554 38.87185,
      -77.0549 38.87098,
      -77.0558 38.87028 
    )
  )',
  4326
);
SELECT 
  @Pentagon AS Shape,
  @Pentagon.NumRings() AS NumRings;

-- Isolating a single ring from a geography instance
DECLARE @Pentagon geography;
SET @Pentagon = geography::STPolyFromText(
  'POLYGON(
    (
      -77.05322 38.87086,
      -77.05468 38.87304,
      -77.05788 38.87280,
      -77.05849 38.87022,
      -77.05556 38.86906,
      -77.05322 38.87086 
    ),
    (
      -77.05582 38.87028,
      -77.05693 38.87073,
      -77.05673 38.87170,
      -77.05547 38.87185,
      -77.05492 38.87098,
      -77.05582 38.87028 
    )
  )',
  4326
);
SELECT
  @Pentagon AS Shape,
  @Pentagon.RingN(1) AS Ring1,
  @Pentagon.RingN(1).STAsText() AS WKT;

-- Counting the number of elements in a geometry collection
DECLARE @Collection geometry;
SET @Collection = geometry::STGeomFromText('
  GEOMETRYCOLLECTION(
    MULTIPOINT((32 2), (23 12)),
    LINESTRING(30 2, 31 5),
    POLYGON((20 2, 23 2.5, 21 3, 20 2))
  )',
  0);
SELECT 
  @Collection AS Shape,
  @Collection.STNumGeometries() AS NumGeometries;

-- Isolating an individual geometry from a collection
DECLARE @DFWRunways geography;
SET @DFWRunways = geography::STMLineFromText(
  'MULTILINESTRING(
    (-97.0214781 32.9125542, -97.0008442 32.8949814),
    (-97.0831328 32.9095756, -97.0632761 32.8902694),
    (-97.0259706 32.9157078, -97.0261717 32.8788783),
    (-97.0097789 32.8983206, -97.0099086 32.8749594),
    (-97.0298833 32.9157222, -97.0300811 32.8788939),
    (-97.0507357 32.9157992, -97.0509261 32.8789717),
    (-97.0546419 32.9158147, -97.0548336 32.8789861)
  )', 4326);
SELECT
  @DFWRunways AS Shape,
  @DFWRunways.STGeometryN(3) AS Geometry3,
  @DFWRunways.STGeometryN(3).STAsText() AS WKT;

-- Measuring the length of a geometry
DECLARE @RoyalMile geography;
SET @RoyalMile = geography::STLineFromText(
  'LINESTRING(-3.20001 55.94821, -3.17227 55.9528)', 4326);
SELECT
  @RoyalMile AS Shape,
  @RoyalMile.STLength() AS Length;

-- Calculating the area contained within a geometry
DECLARE @Cost money = 80000;
DECLARE @Plot geometry;
SET @Plot = geometry::STPolyFromText(
  'POLYGON((633000 4913260, 633000 4913447, 632628 4913447, 632642 4913260, 
    633000 4913260))',
  32631);
SELECT
  @Plot AS Shape,
  @Cost / @Plot.STArea() AS PerUnitAreaCost;

-- Retrieving the SRID of an instance
CREATE TABLE #Imported_Data (
  Location geometry
);
INSERT INTO #Imported_Data VALUES
 (geometry::STGeomFromText('LINESTRING(122 74, 123 72)', 0)),
 (geometry::STGeomFromText('LINESTRING(140 65, 132 63)', 0));

-- Updating the SRID
UPDATE #Imported_Data
 SET Location.STSrid = 32731;

