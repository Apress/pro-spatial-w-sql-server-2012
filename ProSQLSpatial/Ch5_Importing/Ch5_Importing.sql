/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 5 : Importing
 */


/**
 * Before running these code listings, import the esa7day-M1.txt file
 */

-- Check the imported earthquake data
SELECT * FROM eqs7dayM1;

-- Option #1 - Add a computed geography column
ALTER TABLE eqs7dayM1
ADD Epicenter AS geography::Point(Lat, Lon, 4326); 

-- Option #2 - Populate a new geography column
ALTER TABLE eqs7dayM1
ADD Hypocenter geography;
GO

UPDATE eqs7dayM1
SET Hypocenter = 
  geography::STPointFromText(
    'POINT('
      + CAST(Lon AS varchar(255)) + ' '
      + CAST(Lat AS varchar(255)) + ' '
      + CAST (-Depth AS varchar(255)) + ')',
    4326);
GO

-- Option #3 - Add a persisted computed geography column
ALTER TABLE eqs7dayM1
ADD Hypocenter_Persisted AS geography::STPointFromText(
  'POINT('
    + CAST(Lon AS varchar(255)) + ' '
    + CAST(Lat AS varchar(255)) + ' '
    + CAST (-Depth AS varchar(255)) + ')',
  4326) PERSISTED;

/**
 * Before running these code listings, import the precincts shapefile
 */

-- Check the SRID of the imported data
SELECT ogr_geometry.STSrid FROM precincts;

-- Update the SRID to match that in the supplied .PRJ file
UPDATE precincts
SET ogr_geometry.STSrid = 2249;

-- Find the appropriate SRID for the ZCTA shapefile
SELECT spatial_reference_id 
FROM sys.spatial_reference_systems 
WHERE well_known_text LIKE 'GEOGCS%"NAD83"%';

-- Find the appropriate SRID for the Australian River Basins file
SELECT * FROM sys.spatial_reference_systems
WHERE well_known_text LIKE '%AGD66%';



