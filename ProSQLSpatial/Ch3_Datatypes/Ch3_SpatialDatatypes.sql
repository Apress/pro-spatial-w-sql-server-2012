/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 3 : Spatial Datatypes
 */

-- Creating a single geometry Point using the Parse() static method
SELECT geometry::Parse('POINT(30 40)');

-- Create a table with a geography column and populate it using the Point() static method
CREATE TABLE geographypoints (
  Location geography
);

INSERT INTO geographypoints VALUES
(geography::Point(1, 51, 4326)),
(geography::Point(-2, 52, 4326)),
(geography::Point(-1.1, 50.7, 4326));

-- Use the ToString() instance method to retrieve WKT of a geography column
SELECT
  Location.ToString()
FROM
  geographypoints;

-- Use the STBuffer() instance method to buffer a geometry instance
DECLARE @point geometry = geometry::Point(12, 7, 0);
SELECT @point.STBuffer(5);
GO

-- Chain instance methods together - create a buffer and then calculate its area
DECLARE @point geometry = geometry::Point(3, 5, 0);
SELECT @point.STBuffer(5).STArea();

-- Retrieve latitude and longitude properties of a column of geography Points
SELECT
  Location.Lat,
  Location.Long
FROM 
  geographypoints;

-- Some properties are read-only. This will error:
UPDATE geographypoints SET Location.Lat = 20;

-- Some properties can be set. This is ok:
UPDATE geographypoints SET Location.STSrid = 4269;

-- Check the unit of measure for a geography spatial reference system
SELECT 
  unit_of_measure 
FROM 
  sys.spatial_reference_systems 
WHERE 
  authority_name = 'EPSG'
  AND
  authorized_spatial_reference_id = 4326;

-- Calculate the distance between two geography Points
DECLARE @Paris geography = geography::Point(48.87, 2.33, 4326);
DECLARE @Berlin geography = geography::Point(52.52, 13.4, 4326);
SELECT @Paris.STDistance(@Berlin);

-- Defining a different SRID does not change numerical results of geometry datatype
DECLARE @WKT nvarchar(max) = 'LINESTRING (325156 673448, 326897 673929)';
-- SRID 27700:
DECLARE @RoyalMile geometry = geometry::STGeomFromText(@WKT, 27700);
SELECT @RoyalMile.STLength();
-- SRID 32039:
DECLARE @RoyalMile2 geometry = geometry::STGeomFromText(@WKT, 32039);
SELECT @RoyalMile2.STLength(); 

-- Internal Data Structure
SELECT geography::Point(40, -100, 4269);
GO
--0x - Hexadecimal Identifier
--AD100000 - SRID (4269)
--01 - Version
--0C - Properties (valid, single point)
--0000000000004440 - Latitude
--00000000000059C0 - Longitude

-- Building a point up from binary
DECLARE @point geography =
  0xE6100000 +              -- SRID (4326)
  0x02 +                    -- Version (2)
  0x0C +                    -- Properties (Single Point [8] + Valid [4])
  0x0000000000004540 +      -- Latitude (42)
  0x00000000008056C0;       -- Longitude (–90)
SELECT
  @point.STSrid,
  @point.ToString();


-- Can't convert directly between datatypes. This will error:
DECLARE @geog geography;
SET @geog = geography::STGeomFromText('POINT(23 32)', 4326);
SELECT CAST(@geog AS geometry);
GO

-- Instead, can convert between datatypes via WKB
DECLARE @geog geography;
SET @geog = geography::Point(23,32, 4326);
DECLARE @geom geometry;
SET @geom = geometry::STGeomFromWKB(@geog.STAsBinary(), @geog.STSrid);
GO

-- Creating spatially-enabled tables
CREATE TABLE dbo.cities (
  CityName varchar(255),
  CityLocation geography
);

-- Adding spatial column to existing table
CREATE TABLE dbo.customer (
  CustomerID int,
  FirstName varchar(50),
  Surname varchar (50),
  Address varchar (255),
  Postcode varchar (10),
  Country varchar(32)
);
ALTER TABLE dbo.customer
ADD CustomerLocation geography;

-- Enforcing a common SRID
ALTER TABLE dbo.customer
ADD CONSTRAINT enforce_customerlocation_srid4199 
CHECK (CustomerLocation.STSrid = 4199);

-- Enforcing only certain geometry types
ALTER TABLE dbo.customer
ADD CONSTRAINT enforce_customerlocation_point 
CHECK (CustomerLocation.STGeometryType() = 'POINT');







