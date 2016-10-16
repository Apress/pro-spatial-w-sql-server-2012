using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems.Transformations;

namespace Ch8_Transformation
{
  class TransformGeometryToGeographySink : IGeometrySink
  {
    private readonly ICoordinateTransformation _trans;
    private readonly IGeographySink _sink;

    public TransformGeometryToGeographySink(ICoordinateTransformation trans, IGeographySink sink)
    {
      _trans = trans;
      _sink = sink;
    }

    public void BeginGeometry(OpenGisGeometryType type)
    {
      _sink.BeginGeography((OpenGisGeographyType)type);
    }

    public void EndGeometry()
    {
      _sink.EndGeography();
    }

    public void BeginFigure(double x, double y, double? z, double? m)
    {
      double[] fromPoint = { x, y };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double longitude = toPoint[0];
      double latitude = toPoint[1];
      _sink.BeginFigure(latitude, longitude, z, m);
    }

    public void AddLine(double x, double y, double? z, double? m)
    {
      double[] fromPoint = { x, y };
      double[] toPoint = _trans.MathTransform.Transform(fromPoint);
      double longitude = toPoint[0];
      double latitude = toPoint[1];
      _sink.AddLine(latitude, longitude, z, m);
    }

    public void EndFigure()
    {
      _sink.EndFigure();
    }

    public void SetSrid(int srid)
    {
      // Input argument not used since a new srid is defined in the constructor.
      //_sink.SetSrid(srid);
    }
  }
}
