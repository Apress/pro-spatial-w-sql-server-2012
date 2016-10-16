/**
 * Pro Spatial with SQL Server 2012
 * Alastair Aitchison
 * Chapter 7 : Precision, Validity, and Errors
 */

 /**
 * PRECISION
 */

-- Supplying excess precision does not lead to greater than 64bit accuracy
DECLARE @Precise geometry;
SET @Precise = geometry::Point(10.234567890123456789012345678901, 0, 0);
DECLARE @SuperPrecise geometry;
SET @SuperPrecise = geometry::Point(10.234567890123456789012345678901234567, 0, 0);
SELECT @Precise.STEquals(@SuperPrecise);

-- Supplying different precision does not affect storage requirements
DECLARE @LowPrecision geometry;
SET @LowPrecision = geometry::STPointFromText('POINT(1 2)', 0);
DECLARE @HighPrecision geometry;
SET @HighPrecision = geometry::STPointFromText('POINT(1.2345678901234567890123456789  2.3456789012345678)', 0);
SELECT
  DATALENGTH(@LowPrecision),
  DATALENGTH(@HighPrecision);  

-- Calculate distance covered by 1 degree of longitude at the equator - 111km
DECLARE @EquatorA geography = geography::Point(0,0,4326);
DECLARE @EquatorB geography = geography::Point(0,1,4326);
SELECT @EquatorA.STDistance(@EquatorB);  

-- Distance covered by 1 degree of longitude at Tropic of Cancer - 102km
DECLARE @TropicOfCancerA geography = geography::Point(23.5,0,4326);
DECLARE @TropicOfCancerB geography = geography::Point(23.5,1,4326);
SELECT @TropicOfCancerA.STDistance(@TropicOfCancerB);

-- Distance covered by 1 degree of longitude at the Arctic Circle - only 44km
DECLARE @ArcticCircleA geography = geography::Point(66.5,0,4326);
DECLARE @ArcticCircleB geography = geography::Point(66.5,1,4326);
SELECT @ArcticCircleA.STDistance(@ArcticCircleB);
GO

-- Import the Ch7 assembly then register the RoundGeography function
CREATE FUNCTION dbo.RoundGeography (
  @g geography,
  @precision int
) RETURNS geography
AS EXTERNAL NAME
ProSpatialCh7.[ProSpatial.Ch7.UserDefinedFunctions].RoundGeography;

-- Test the RoundGeography function
DECLARE @EiffelTower geography = 'POINT(2.2945117950439298 48.858259942745526)';
DECLARE @RoundedEiffelTower geography = dbo.RoundGeography(@EiffelTower, 5);
SELECT
  @EiffelTower.ToString() AS WKT,
  DATALENGTH(@EiffelTower.ToString()) AS Length
UNION ALL
SELECT
  @RoundedEiffelTower.ToString() AS WKT,
  DATALENGTH(@RoundedEiffelTower.ToString()) AS Length;
-- What is the effect on accuracy?
SELECT @EiffelTower.STDistance(@RoundedEiffelTower);

-- Calculation Precision. Consider the following two lines
DECLARE @line1 geometry = 'LINESTRING(0 13, 431 310)';
DECLARE @line2 geometry = 'LINESTRING(0 502, 651 1)';
-- Where do the two lines cross?
SELECT @line1.STIntersection(@line2).ToString();
-- But what about if we run a query like this?
SELECT
  @line1.STIntersection(@line2).STIntersects(@line1),   --0 Huh?
  @line1.STIntersection(@line2).STIntersects(@line2);   --0 Huh?

-- Another example
DECLARE @square geometry = 'POLYGON((0 0, 100 0, 100 100, 0 100, 0 0))';
DECLARE @rectangle geometry = 'POLYGON((-10 5, 10 5, 10 15, -10 15, -10 5))';
SELECT
  @rectangle.STIntersects(@square),
  @rectangle.STIntersection(@square).STArea(); -- 99.9999999995 - why not 100?
GO

-- What about if we make the square bigger?
DECLARE @rectangle geometry = 'POLYGON((-10 5, 10 5, 10 15, -10 15, -10 5))';
DECLARE @square geometry = 'POLYGON((0 0, 1000 0, 1000 1000, 0 1000, 0 0))';
SELECT @rectangle.STIntersection(@square).STArea();
-- 99.9999999999713

DECLARE @square2 geometry = 'POLYGON((0 0, 100000 0, 100000 100000, 0 100000, 0 0))';
SELECT @rectangle.STIntersection(@square2).STArea();
-- 99.9999999962893
 
DECLARE @square3 geometry = 'POLYGON((0 0, 1e9 0, 1e9 1e9, 0 1e9, 0 0))';
SELECT @rectangle.STIntersection(@square3).STArea();
-- 99.9999690055833
 
DECLARE @square4 geometry = 'POLYGON((0 0, 1e12 0, 1e12 1e12, 0 1e12, 0 0))';
SELECT @rectangle.STIntersection(@square4).STArea();
-- 99.9691756255925

DECLARE @square5 geometry = 'POLYGON((0 0, 1e15 0, 1e15 1e15, 0 1e15, 0 0))';
SELECT @rectangle.STIntersection(@square5).STArea();
GO
-- 67.03125

-- A  function to compare whether two LineStrings are the same
CREATE FUNCTION CompareLineStrings (@l1 geometry, @l2 geometry)
RETURNS bit AS
BEGIN
-- Only test LineString geometries
IF NOT (@l1.STGeometryType() = 'LINESTRING' AND @l2.STGeometryType() = 'LINESTRING')
RETURN NULL
-- Startpoints differ by more than 1 unit
IF @l1.STStartPoint().STDistance(@l2.STStartPoint()) > 1
RETURN 0
-- Endpoints differs by more than 1 unit
IF @l1.STEndPoint().STDistance(@l2.STEndPoint()) > 1
RETURN 0
-- Length differs by more than 5%
IF ABS(@l1.STLength() - @l2.STLength() / @l1.STLength()) > 0.05
RETURN 0
-- Any part of l2 lies more than 0.1 units from l1
IF @l1.STBuffer(0.1).STDifference(@l2).STEquals('GEOMETRYCOLLECTION EMPTY') = 0
RETURN 0
-- All tests pass, so return success
RETURN 1
END


/**
 * VALIDITY
 */
-- An erroneous geometry - will error
DECLARE @LineMustHave2Points geometry;
SET @LineMustHave2Points = geometry::STLineFromText('LINESTRING(3 2)', 0);

-- Another erroneous geometry
DECLARE @PolygonMustHave4Points geometry;
SET @PolygonMustHave4Points = geometry::STPolyFromText('POLYGON((0 0, 10 2, 0 0))', 0);

-- And another
DECLARE @UnsupportedSRID geography;
SET @UnsupportedSRID = geography::STPointFromText('POINT(52 0)', 123);

-- This geometry will succeed, however it is invalid
DECLARE @SelfIntersectingPolygon geometry;
SET @SelfIntersectingPolygon = 'POLYGON((0 0, 6 0, 3 5, 0 0), (2 2, 8 2, 8 4, 2 4, 2 2))';

-- This is also invalid
DECLARE @InvalidLinestring geometry;
SET @InvalidLinestring = 'LINESTRING(0 0, 10 0, 5 0)';

-- Test whether a geometry is valid
DECLARE @Spike geometry
SET @Spike = geometry::STPolyFromText('POLYGON((0 0,1 1,2 2,0 0))', 0)
SELECT 
  @Spike.STAsText(),
  @Spike.STIsValid();
GO

-- Finding out why a geometry is invalid
DECLARE @g geometry = 'LINESTRING(0 0, 5 10, 8 2)';
DECLARE @h geometry = 'LINESTRING(0 0, 10 0, 5 0)';
DECLARE @i geometry = 'POLYGON((0 0, 2 0, 2 2, 0 2, 0 0), (1 0, 3 0, 3 1, 1 1, 1 0))';
SELECT
  @g.STIsValid() AS STIsValid, @g.IsValidDetailed() AS IsValidDetailed
UNION ALL SELECT
  @h.STIsValid(), @h.IsValidDetailed()
UNION ALL SELECT
  @h.STIsValid(), @i.IsValidDetailed();
GO

-- Making an object valid may affect the start/end point
DECLARE @InvalidLinestring geometry;
SET @InvalidLinestring = 'LINESTRING(0 0, 10 0, 5 0)';
SELECT @InvalidLinestring.MakeValid().ToString()

-- Making valid can also change geometry type
DECLARE @Spike geometry = 'POLYGON((0 0,1 1,2 2,0 0))';
SELECT @Spike.MakeValid().ToString()

-- Making valid may cause coordinate values to shift
DECLARE @SelfIntersectingPolygon geometry;
SET @SelfIntersectingPolygon = 'POLYGON((0 0, 6 0, 3 5, 0 0), (2 2, 8 2, 8 4, 2 4, 2 2))';
SELECT @SelfIntersectingPolygon.MakeValid().ToString();


/**
 * HANDLING ERRORS
 */

-- Dissecting an error message
DECLARE @LineMustHave2Points geometry;
SET @LineMustHave2Points = geometry::STLineFromText('LINESTRING(3 2)', 0);

-- Different problem, but produces the same generic T-SQL error 6522
DECLARE @UnsupportedSRID geography;
SET @UnsupportedSRID = geography::STPointFromText('POINT(52 0)', 123);

-- Can't use a common approach as follows because ERROR_NUMBER is always the same
BEGIN TRY
  SELECT geometry::STPolyFromText('POLYGON((0 0, 10 2, 0 0))', 0);
END TRY
BEGIN CATCH
  IF ERROR_NUMBER() = 123
    -- Code to deal with error 123 here
    SELECT 'Error 123 occurred'
  ELSE IF ERROR_NUMBER() = 456
    -- Code to deal with error 456 here
    SELECT 'Error 456 occurred'
  ELSE
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH

-- Instead, can distil the SQLCLR exception from the error message
BEGIN TRY
  SELECT geometry::STPolyFromText('POLYGON((0 0, 10 2, 0 0))', 0);
END TRY
BEGIN CATCH
  -- Has a SQLCLR error occurred?
  IF ERROR_NUMBER() = 6522
  BEGIN
    -- Retrieve the error message
    DECLARE @errorMsg nvarchar(max) = ERROR_MESSAGE();
    DECLARE @exc int;
    -- Distil the SQLCLR exception number from the error message
    SET @exc = SUBSTRING(@errorMsg, PATINDEX('%: 24[0-9][0-9][0-9]%', @errorMsg) + 2, 5); 
    IF @exc = 24305
      -- Code to deal with exception 24305 here
      SELECT 'Exception 24305 occurred';
	      ELSE IF @exc = 24000
      -- Code to deal with exception 24000 here
      SELECT 'Exception 24000 occurred';
    ELSE
      SELECT '...';
  END
END CATCH



