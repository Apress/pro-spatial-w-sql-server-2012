using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Types;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace ProSQLSpatial.Ch7_PrecisionErrors
{
  class RoundGeography : IGeographySink
  {
    private readonly IGeographySink _target;  // the target sink
    private readonly int _precision;      // the number of fractional digits in the return value

    public RoundGeography(int precision, IGeographySink target)
    {
      _target = target;
      _precision = precision;
    }

    public void SetSrid(int srid)
    {
      _target.SetSrid(srid);
    }

    public void BeginGeography(OpenGisGeographyType type)
    {
      _target.BeginGeography(type);
    }

    // Each BeginFigure call rounds the start point to the required precision.
    public void BeginFigure(double x, double y, double? z, double? m)
    {
      _target.BeginFigure(Math.Round(x, _precision), Math.Round(y, _precision), z, m);
    }

    // Each AddLine call rounds subsequent points to the required precision.
    public void AddLine(double x, double y, double? z, double? m)
    {
      _target.AddLine(Math.Round(x, _precision), Math.Round(y, _precision), z, m);
    }

    public void EndFigure()
    {
      _target.EndFigure();
    }

    public void EndGeography()
    {
      _target.EndGeography();
    }
  }

  // Create a wrapper function
  public partial class UserDefinedFunctions
  {
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlGeography RoundGeography(SqlGeography g, Int32 precision)
    {
      SqlGeographyBuilder constructed = new SqlGeographyBuilder();
      RoundGeography rounded = new RoundGeography(precision, constructed);
      g.Populate(rounded);
      return constructed.ConstructedGeography;
    }
  }

}