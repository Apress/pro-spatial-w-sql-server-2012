using System;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems.Transformations;

namespace Ch8_Transformation
{
  class TransformGeographyToGeographySink : IGeographySink
  {
    private readonly ICoordinateTransformation _trans;
    private readonly IGeographySink _sink;

    public TransformGeographyToGeographySink(ICoordinateTransformation trans, IGeographySink sink)
    {
      _trans = trans;
      _sink = sink;
    }

    public void BeginGeography(OpenGisGeographyType type)
    {
      _sink.BeginGeography(type);
    }

    public void EndGeography()
    {
      _sink.EndGeography();
    }

    public void BeginFigure(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
    {
      double[] fromPoint = { longitude, latitude };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double tolong = toPoint[0];
      double tolat = toPoint[1];
      _sink.BeginFigure(tolat, tolong, z, m);
    }

    public void AddLine(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
    {
      double[] fromPoint = { longitude, latitude };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double tolong = toPoint[0];
      double tolat = toPoint[1];
      _sink.AddLine(tolat, tolong, z, m);
    }

    public void EndFigure()
    {
      _sink.EndFigure();
    }

    public void SetSrid(int srid)
    {
      // _sink.SetSrid(srid);
    }

  }
}
