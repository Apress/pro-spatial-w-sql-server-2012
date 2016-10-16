/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 18 : Indexing
 */

-- Create a simple table
CREATE TABLE Points (
  id char(1) NOT NULL,
  shape geometry
); 

-- Add a clustered primary key
ALTER TABLE Points 
ADD CONSTRAINT idxCluster PRIMARY KEY CLUSTERED (id ASC);
GO 

-- Create a spatial index
CREATE SPATIAL INDEX sidxPoints ON Points(shape)
USING GEOMETRY_GRID WITH (
BOUNDING_BOX = (0, 0, 4096, 4096),
GRIDS = (
  LEVEL_1 = MEDIUM,
  LEVEL_2 = MEDIUM,
  LEVEL_3 = MEDIUM,
  LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16); 
GO

-- Insert a few rows of data
INSERT INTO Points VALUES
('A', geometry::Point(0.5, 2.5, 0)),
('B', geometry::Point(2.5, 1.5, 0)),
('C', geometry::Point(3.25, 0.75, 0)),
('D', geometry::Point(3.75, 2.75, 0)); 
GO

-- Use the index in a query
DECLARE @Polygon geometry='POLYGON((1.5 0.5, 3.5 0.5, 3.5 2.5, 1.5 2.5, 1.5 0.5))'; 
SELECT id
FROM Points WITH(INDEX(sidxPoints))
WHERE shape.STIntersects(@Polygon) = 1; 
GO

-- Understand how the index is being used with sp_help_spatial_geometry_index
EXEC sp_help_spatial_geometry_index
@tabname = Points,
@indexname = sidxPoints,
@verboseoutput=0,
@query_sample='POLYGON((1.5 0.5, 3.5 0.5, 3.5 2.5, 1.5 2.5, 1.5 0.5))' ;

-- Designing queries to use a spatial index
CREATE TABLE IndexTest (
  id int NOT NULL,
  geom geometry,
  CONSTRAINT pk_IndexTest PRIMARY KEY CLUSTERED (id ASC)
);
CREATE SPATIAL INDEX sidx_IndexTest ON IndexTest(geom)
WITH ( BOUNDING_BOX = (0, 0, 10, 10) ); 

-- Spatial index can be used for this query
SELECT * FROM IndexTest
WHERE geom.STIntersects('POINT(3 2)') = 1; 

-- But not for this one
SELECT * FROM IndexTest
WHERE geom.STLength() > 100; 

-- If you want to do queries like the above efficiently,
-- consider adding a persisted computed column
ALTER TABLE IndexTest ADD geom_length AS geom.STLength() PERSISTED;
CREATE INDEX idx_geom_length ON IndexTest(geom_length); 

-- Query syntax is important. This can use a spatial index:
SELECT * FROM IndexTest WHERE geom.STEquals('POINT(3 2)') = 1;  
-- Whereas this query cannot:
SELECT * FROM IndexTest WHERE 1 = geom.STEquals('POINT(3 2)');  

-- Sometimes necessary to add an index hint
SELECT * FROM IndexTest WITH(INDEX(sidx_IndexTest))
WHERE geom.STIntersects('POINT(3 2)') = 1; 
