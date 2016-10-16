using System;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems.Transformations;

namespace Ch8_Transformation
{
  class TransformGeometryToGeometrySink : IGeometrySink
  {
    private readonly ICoordinateTransformation _trans;
    private readonly IGeometrySink _sink;

    public TransformGeometryToGeometrySink(ICoordinateTransformation trans, IGeometrySink sink)
    {
      _trans = trans;
      _sink = sink;
    }

    public void BeginGeometry(OpenGisGeometryType type)
    {
      _sink.BeginGeometry(type);
    }

    public void EndGeometry()
    {
      _sink.EndGeometry();
    }

    public void BeginFigure(double x, double y, Nullable<double> z, Nullable<double> m)
    {
      double[] fromPoint = { x, y };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double tox = toPoint[0];
      double toy = toPoint[1];
      _sink.BeginFigure(tox, toy, z, m);
    }

    public void AddLine(double x, double y, Nullable<double> z, Nullable<double> m)
    {
      double[] fromPoint = { x, y };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double tox = toPoint[0];
      double toy = toPoint[1];
      _sink.AddLine(tox, toy, z, m);
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
