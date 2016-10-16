using System;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems.Transformations;

namespace Ch8_Transformation
{
  class TransformGeographyToGeometrySink : IGeographySink
  {
    private readonly ICoordinateTransformation _trans;
    private readonly IGeometrySink _sink;

    public TransformGeographyToGeometrySink(ICoordinateTransformation trans, IGeometrySink sink)
    {
      _trans = trans;
      _sink = sink;
    }

    public void BeginGeography(OpenGisGeographyType type)
    {
      _sink.BeginGeometry((OpenGisGeometryType)type);
    }

    public void EndGeography()
    {
      _sink.EndGeometry();
    }

    public void BeginFigure(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
    {
      double[] fromPoint = { longitude, latitude };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double x = toPoint[0];
      double y = toPoint[1];
      _sink.BeginFigure(x, y, z, m);
    }

    public void AddLine(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
    {
      double[] fromPoint = { longitude, latitude };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double x = toPoint[0];
      double y = toPoint[1];
      _sink.AddLine(x, y, z, m);
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
