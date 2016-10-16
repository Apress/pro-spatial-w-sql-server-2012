/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 4 : Creating
 */


-- STPointFromText() method can be used to create Point instances:
SELECT
geography::STPointFromText('POINT(153 -27.5)', 4326);
GO

-- STLineFromText() method can be used to create LineString instances
SELECT
geometry::STLineFromText('LINESTRING(300500 600150, 310200 602500)', 27700);

-- STGeomFromText() can create any sort of valid geometry from WKT
SELECT
geography::STGeomFromText('POINT(153 -27.5)', 4326),
geometry::STGeomFromText('LINESTRING(300500 600150, 310200 602500)', 27700);

-- Parse() method accepts WKT
DECLARE @Delhi geography = 'POINT(77.25 28.5)';
GO

-- ...is the same as
DECLARE @Delhi geography = geography::Parse('POINT(77.25 28.5)');
GO

-- ... is the same as 
DECLARE @Delhi geography = geography::STGeomFromText('POINT(77.25 28.5)', 4326);

-- Different ways to retrieve the WKT of an existing instance
DECLARE @Point geometry = geometry::STPointFromText('POINT(14 9 7)', 0);
SELECT
  @Point.STAsText() AS  STAsText,
  @Point.AsTextZM() AS AsTextZM,
  @Point.ToString() AS ToString;

-- Similar methods exist for WKB. E.g. STPointFromWKB to create Points
SELECT
geometry::STPointFromWKB(0x00000000014001F5C28F5C28F6402524DD2F1A9FBE, 2099);

-- and STGeomFromWKB to create any kind of geometry
SELECT
geometry::STGeomFromWKB(0x00000000014001F5C28F5C28F6402524DD2F1A9FBE, 2099);

-- Use STAsBinary() to retrieve the WKB representation of an existing geometry
DECLARE @g geometry = geometry::STPointFromText('POINT(14 9 7)', 0);
SELECT
  @g.STAsBinary();

-- Use AsBinaryZM() to retrieve the WKB representation retaining Z and M coordinates
DECLARE @g geometry = geometry::STPointFromText('POINT(14 9 7)', 0);
SELECT @g.AsBinaryZM();

-- Use GeomFromGml() to create any kind of geometry from GML
DECLARE @gml xml = 
'<Point xmlns="http://www.opengis.net/gml">
  <pos>47.6 -122.3</pos>
</Point>';
SELECT
geography::GeomFromGml(@gml, 4269);

-- Note the importance of stating the namespace. This will error:
DECLARE @NoGMLNameSpace xml = 
'<LineString>
  <posList>-6 4 3 -5</posList>
</LineString>';
SELECT geometry::GeomFromGml(@NoGMLNameSpace, 0);

-- Use AsGml() to retrieve GML representation of an existing instance
DECLARE @polygon geography = 'POLYGON((-4 50, 2 50, 2 60, -4 60, -4 50))';
SELECT @polygon.AsGml();

-- Dynamically-generated WKT
CREATE TABLE GPSLog (
  Latitude float,
  Longitude float,
  LogTime datetime
);
INSERT INTO GPSLog VALUES
  (51.868, -1.198, '2011-06-02T13:47:00'),
  (51.857, -1.182, '2011-06-02T13:48:00'),
  (51.848, -1.167, '2011-06-02T13:49:00'),
  (51.841, -1.143, '2011-06-02T13:50:00'),
  (51.832, -1.124, '2011-06-02T13:51:00');
-- Create a WKT string and pass it to the STGeomFromText() method?
SELECT geography::STGeomFromText(
  'POINT(' + CAST(Longitude AS varchar(32)) + ' ' + CAST(Latitude AS varchar(32)) + ')',
  4326
  )
FROM GPSLog;
-- Simpler to just use the Point() method
SELECT geography::Point(Latitude, Longitude, 4326);

-- Append coordinates into a LineString representation
DECLARE @WKT nvarchar(max) = '';
SELECT @WKT = @WKT + CAST(Latitude AS varchar(32)) + ' ' + CAST(Longitude AS varchar(32)) + ','
FROM GPSLog
ORDER BY LogTime;
-- Remove the final trailing comma
SET @WKT = LEFT(@WKT, LEN(@WKT) - 1);
-- Append the LINESTRING keyword and enclose the coordco-ordinate list in brackets
SET @WKT = 'LINESTRING(' + @WKT + ')';
-- Pass the constructed WKT to the static method
SELECT geography::STGeomFromText(@WKT, 4326);
