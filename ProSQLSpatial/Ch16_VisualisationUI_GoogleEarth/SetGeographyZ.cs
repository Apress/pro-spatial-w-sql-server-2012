using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SqlServer.Types;

namespace ProSQLSpatial
{
  public partial class Ch16_Visualisation 
  {

    // Make our ShiftGeographySink into a function call by hooking it into a simple pipeline.
    public static SqlGeography SetGeographyZ(SqlGeography g, double z)
    {
      // create a sink that will create a Geography instance
      SqlGeographyBuilder b = new SqlGeographyBuilder();

      // create a sink to do the shift and plug it in to the builder
      ShiftGeographySink s = new ShiftGeographySink(z, b);

      // plug our sink into the Geography instance and run the pipeline
      g.Populate(s);

      // the end of our pipeline is now populated with the shifted Geography instance
      return b.ConstructedGeography;
    }

    /**
    * This class implements a Geography sink that will shift an input Geography by a given amount in the x and
    * y directions.  It directs its output to another sink, and can therefore be used in a pipeline if desired.
    */
    public class ShiftGeographySink : IGeographySink110
    {
      private readonly IGeographySink110 _target;  // the target sink
      private readonly double _z;         // How much to shift in the x direction.

      // We take an amount to shift in the x and y directions, as well as a target sink, to which
      // we will pipe our result.
      public ShiftGeographySink(double z, IGeographySink110 target)
      {
        _target = target;
        _z = z;
      }

      // Just pass through without change.
      public void SetSrid(int srid)
      {
        _target.SetSrid(srid);
      }

      // Just pass through without change.
      public void BeginGeography(OpenGisGeographyType type)
      {
        _target.BeginGeography(type);
      }

      // Each BeginFigure call will just move the start point by the required amount.
      public void BeginFigure(double x, double y, double? z, double? m)
      {
        _target.BeginFigure(x, y, _z, m);
      }

      // Each AddLine call will just move the endpoint by the required amount.
      public void AddLine(double x, double y, double? z, double? m)
      {
        _target.AddLine(x, y, _z, m);
      }

      // Each AddLine call will just move the endpoint by the required amount.
      public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
      {
        _target.AddCircularArc(x1, y1, _z, m1, x2, y2, _z, m2);
      }

      // Just pass through without change.
      public void EndFigure()
      {
        _target.EndFigure();
      }

      // Just pass through without change.
      public void EndGeography()
      {
        _target.EndGeography();
      }
    }





  }
}
