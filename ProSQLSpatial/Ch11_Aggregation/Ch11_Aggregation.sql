/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 11 : Aggregation
 */

-- Creating a union of two geometries
DECLARE @NorthIsland geography;
SET @NorthIsland = geography::STPolyFromText(
  'POLYGON((175.3 -41.5, 178.3 -37.9, 172.8 -34.6, 175.3 -41.5))',
  4326);
DECLARE @SouthIsland geography;
SET @SouthIsland = geography::STPolyFromText(
  'POLYGON((169.3 -46.6, 174.3 -41.6, 172.5 -40.7, 166.3 -45.8, 169.3 -46.6))',
  4326);
DECLARE @NewZealand geography = @NorthIsland.STUnion(@SouthIsland);
SELECT @NewZealand;

-- Creating different sorts of unions
DECLARE @table TABLE (
  geomA geometry,
  geomB geometry
);
INSERT INTO @table VALUES
('POINT(0 0)', 'POINT(2 2)'),
('POINT(0 0)', 'POINT(0 0)'),
('POINT(5 2)', 'LINESTRING(5 2, 7 9)'),
('LINESTRING(0 0, 5 2)', 'CIRCULARSTRING(5 2, 6 3, 9 2)'),
('POLYGON((0 0, 3 0, 3 3, 0 3, 0 0))', 'POLYGON((0 3, 3 3, 1 5, 0 3))'),
('POINT(0 0)', 'LINESTRING(2 2, 5 4)');
SELECT
  geomA.ToString(),
  geomB.ToString(),
  geomA.STUnion(geomB).ToString()
FROM @table;

-- Note that A.STUnion(B) does not simply append points of B onto end of A
DECLARE @a_to_b geometry = geometry::STLineFromText('LINESTRING(0 0, 5 2)', 0);
DECLARE @b_to_c geometry = geometry::STLineFromText('LINESTRING(5 2, 8 6)', 0);
SELECT @a_to_b.STUnion(@b_to_c).ToString();

-- Using STDifference() to subtract one geometry from another
DECLARE @Radar geography;
SET @Radar = geography::STMPointFromText(
  'MULTIPOINT(
    -2.597 52.398, -2.289 53.755, -0.531 51.689, -6.340 54.500, -5.223 50.003,
    -0.559 53.335, -4.445 51.980, -4.231 55.691, -2.036 57.431, -6.183 58.211,
    -3.453 50.963, 0.604 51.295,  -1.654 51.031, -2.199 49.209, -6.259 53.429,
    -8.923 52.700)', 4326);
-- Buffer each station to obtain the 75km coverage area
DECLARE @RadarCoverage geography
SET @RadarCoverage = @Radar.BufferWithCurves(75000);
-- Declare an approximate shape of the British Isles
DECLARE @BritishIsles geography;
SET @BritishIsles = geography::STMPolyFromText(
  'MULTIPOLYGON(
    ((0.527 52.879, -3.164 56.0197, -1.626 57.631, -4.087 57.654, -2.989 58.582, 
    -5.0977 58.514, -6.504 56.240, -4.746 54.670, -3.516 54.848, -3.252 53.432, 
    -4.614 53.301, -4.922 51.697, -3.12 51.505, -5.625 50.032, 1.626 51.286, 
    0.791 51.423, 1.890 52.291, 1.274 52.959, 0.527 52.879)),
    ((-6.548 52.123, -5.317 54.518, -7.734 55.276, -9.976 53.354, -9.888 51.369, 
    -6.548 52.123)))', 4326);
-- Calculate the difference between the British Isles and the area of radar coverage
SELECT 
  @BritishIsles.STDifference(@RadarCoverage);

-- Calculating the symmetric difference between two geometries
DECLARE @KWEST geography, @KEAST geography;
SET @KWEST = geography::Point(41.86, -87.88, 4269).BufferWithCurves(10000);
SET @KEAST = geography::Point(41.89, -87.79, 4269).BufferWithCurves(8000);
SELECT @KEAST.STSymDifference(@KWEST);

-- Determining the intersection between two geometries
DECLARE @Marshes geography;
SET @Marshes = geography::STPolyFromText(
  'POLYGON(( 
    12.94 41.57, 12.71 41.46, 12.91 41.39, 13.13 41.26, 13.31 41.33, 12.94 41.57))',
  4326);
-- Declare the road
DECLARE @ViaAppia geography;
SET @ViaAppia = geography::STLineFromText(
  'LINESTRING(
    12.51 41.88, 13.25 41.28, 13.44 41.35, 13.61 41.25, 13.78 41.23, 13.89 41.11, 
    14.22 41.10, 14.47 41.02, 14.79 41.13, 14.99 41.04, 15.48 40.98, 15.82 40.96, 
    17.19 40.51, 17.65 40.50, 17.94 40.63)',
  4326);
-- Determine that section of road that passes through the marshes
SELECT @ViaAppia.STIntersection(@Marshes);

-- Aggregating columns of data
CREATE TABLE #BunchOLines (
  line geography
);
INSERT INTO #BunchOLines VALUES
  ('LINESTRING(0 52, 1 53)'),
  ('LINESTRING(1 53, 1 54, 2 54)'),
  ('LINESTRING(2 54, 4 54)'),
  ('LINESTRING(2 54, 0 55, -1 55)');

-- Before introduction of aggregate functions, could use an approach like this:
DECLARE @g geography = 'LINESTRING EMPTY';
SELECT @g = @g.STUnion(line) FROM #BunchOLines;
SELECT @g.STAsText();

-- Create a UnionAggregate()
SELECT geography::UnionAggregate(line).STAsText()
FROM #BunchOLines;

-- Creating an EnvelopeAggregate()
SELECT geography::EnvelopeAggregate(line).STAsText()
FROM #BunchOLines;

-- Creating a CollectionAggregate()
SELECT geography::CollectionAggregate(line)
FROM #BunchOLines;

-- Creating a ConvexHullAggregate()
SELECT geography::ConvexHullAggregate(line)
FROM #BunchOLines;

-- Combining Spatial Result Sets
DECLARE @MarathonCities table(
  City varchar(32),
  Location geography
);
INSERT INTO @MarathonCities VALUES
('Amsterdam', 'POINT(4.9 52.4)'),
('Athens', 'POINT(23.7 38)'),
('Berlin', 'POINT(13.4 52.5)'),
('Boston', 'POINT(-71.1 42.4)'),
('Chicago', 'POINT(-87.7 41.9)'),
('Honolulu', 'POINT(-157.85 21.3)'),
('London', 'POINT(-0.15 51.5)'),
('New York', 'POINT(-74 40.7)'),
('Paris', 'POINT(2.34 48.8)'),
('Rotterdam', 'POINT(4.46 4.63)'),
('Tokyo', 'POINT(139.7 35.7)'); 
DECLARE @OlympicCities table(
  City varchar(32),
  Location geography
);
INSERT INTO @OlympicCities VALUES
('Sydney', 'POINT(151.2 -33.8)'),
('Athens', 'POINT(23.7 38)'),
('Beijing', 'POINT(116.4 39.9)'),
('London', 'POINT(-0.15 51.5)');

-- Cannot use a UNION query
SELECT City, Location FROM @MarathonCities
UNION
SELECT City, Location FROM @OlympicCities;

-- But can use a UNION ALL
SELECT City, Location FROM @MarathonCities
UNION ALL
SELECT City, Location FROM @OlympicCities;

-- Joining tables together
DECLARE @Quadrants table (
  Quadrant varchar(32),
  QuadrantLocation geography
);
INSERT INTO @Quadrants VALUES
('NW', 'POLYGON((0 0, 0 90, -179.9 90, -179.9 0, 0 0))'),
('NE', 'POLYGON((0 0, 179.9 0, 179.9 90, 0 90, 0 0))'),
('SW', 'POLYGON((0 0, -179.9 0, -179.9 -90, 0 -90, 0 0))'),
('SE', 'POLYGON((0 0, 0 -90, 179.9 -90, 179.9 0, 0 0))');
SELECT
  CityName,
  Quadrant
FROM
  @Cities
  JOIN @Quadrants ON CityLocation.STIntersects(QuadrantLocation) = 1;

