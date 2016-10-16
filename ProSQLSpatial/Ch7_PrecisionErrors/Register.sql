DROP FUNCTION dbo.RoundGeography;
DROP ASSEMBLY ProSQLSpatial;

CREATE ASSEMBLY ProSQLSpatial
FROM 'C:\Users\Alastair\Documents\Visual Studio 2008\Projects\ProSQLSpatial\ProSQLSpatial\bin\Debug\ProSQLSpatial.dll'
WITH PERMISSION_SET = SAFE;
GO

CREATE FUNCTION dbo.RoundGeography(@g geography, @precision int) RETURNS geography AS
EXTERNAL NAME ProSQLSpatial.[ProSQLSpatial.Ch7_PrecisionErrors.UserDefinedFunctions].RoundGeography
GO

