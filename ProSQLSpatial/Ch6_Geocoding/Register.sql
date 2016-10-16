DROP FUNCTION dbo.GeocodeTVF
DROP FUNCTION dbo.Geocode;
DROP FUNCTION dbo.RESTRoute;
DROP ASSEMBLY Ch6_Geocoding;
GO

CREATE ASSEMBLY Ch6_Geocoding
FROM 'C:\ProSQLSpatial\Ch6_Geocoding\bin\Debug\Ch6_Geocoding.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS;
GO

/**
 * REST Geocoder
 */
CREATE FUNCTION dbo.Geocode(
  @countryRegion nvarchar(max),
  @adminDistrict nvarchar(max),
  @locality nvarchar(max),
  @postalCode nvarchar(max),
  @addressLine nvarchar(max)
  ) RETURNS nvarchar(max)
AS EXTERNAL NAME Ch6_Geocoding.[ProSQLSpatial.Ch6.UserDefinedFunctions].GeocodeUDF
GO

SELECT dbo.Geocode('UK', 'Norfolk', 'Norwich', '', '');
GO


/**
 * Table-valued geocoder
 */
CREATE FUNCTION dbo.GeocodeTVF(
  @addressLine nvarchar(255),
  @locality nvarchar(255), 
  @adminDistrict nvarchar(255),
  @postalCode nvarchar(255),
  @countryRegion nvarchar(255)
  ) RETURNS table (Name nvarchar(255), Point geography, BoundingBox geography)
AS EXTERNAL NAME Ch6_Geocoding.[ProSQLSpatial.Ch6.UserDefinedFunctions].GeocodeTVF
GO

SELECT * FROM dbo.GeocodeTVF('', 'Boston', '', '', 'UK');
GO


/**
 * Route finder (from Ch14 but included here as it uses the same Bing Maps API)
 */
CREATE FUNCTION dbo.Route(@Start geography, @End geography, @method nvarchar(max)) RETURNS geography
AS EXTERNAL NAME Ch6_Geocoding.[ProSQLSpatial.Ch14.UserDefinedFunctions].RESTRoute
GO

DECLARE @Start geography = geography::Point(52,0,4326);
DECLARE @End geography = geography::Point(53, -1,4326);
SELECT dbo.Route(@Start, @End, 'DRIVING');
GO