/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 1 : Spatial Reference Systems
 */

-- View details of all supported spatial reference systems for the geography datatype
SELECT *
FROM sys.spatial_reference_systems;

-- Retrieve the well-known text of the WGS84 spatial reference system (SRID 4326)
SELECT
  well_known_text
FROM
  sys.spatial_reference_systems
WHERE
  authority_name = 'EPSG'
  AND 
  authorized_spatial_reference_id = 4326;
