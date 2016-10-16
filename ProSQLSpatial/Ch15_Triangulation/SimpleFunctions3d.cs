using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace ProSQLSpatial
{
  public partial class Ch15_Triangulation
  {

    // Construct a triangle from 3 vertices
    private static SqlGeometry Triangle3dFromPoints(SimplePoint3d p1, SimplePoint3d p2, SimplePoint3d p3, int srid)
    {
      SqlGeometryBuilder TriangleBuilder = new SqlGeometryBuilder();
      TriangleBuilder.SetSrid(srid);
      TriangleBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
      TriangleBuilder.BeginFigure(p1.x, p1.y, p1.z, null);
      TriangleBuilder.AddLine(p2.x, p2.y, p2.z, null);
      TriangleBuilder.AddLine(p3.x, p3.y, p3.z, null);
      TriangleBuilder.AddLine(p1.x, p1.y, p1.z, null);
      TriangleBuilder.EndFigure();
      TriangleBuilder.EndGeometry();
      return TriangleBuilder.ConstructedGeometry;
    }

    private static void CalculateCircumcircle3d(SimplePoint3d p1, SimplePoint3d p2, SimplePoint3d p3, out SimplePoint3d circumCentre, out double radius)
    {
      // Calculate the length of each side of the triangle
      double a = Distance3d(p2, p3); // side a is opposite point 1
      double b = Distance3d(p1, p3); // side b is opposite point 2 
      double c = Distance3d(p1, p2); // side c is opposite point 3

      // Calculate the radius of the circumcircle
      double area = Math.Abs((double)(p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y)) / 2);
      radius = a * b * c / (4 * area);

      // Define area coordinates to calculate the circumcentre
      double pp1 = Math.Pow(a, 2) * (Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2));
      double pp2 = Math.Pow(b, 2) * (Math.Pow(c, 2) + Math.Pow(a, 2) - Math.Pow(b, 2));
      double pp3 = Math.Pow(c, 2) * (Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2));

      // Normalise
      double t1 = pp1 / (pp1 + pp2 + pp3);
      double t2 = pp2 / (pp1 + pp2 + pp3);
      double t3 = pp3 / (pp1 + pp2 + pp3);

      // Convert to Cartesian
      double x = t1 * p1.x + t2 * p2.x + t3 * p3.x;
      double y = t1 * p1.y + t2 * p2.y + t3 * p3.y;

      circumCentre = new SimplePoint3d(x, y, 0);
    }

    // Calculate the distance between two SimplePoints
    private static double Distance3d(SimplePoint3d p1, SimplePoint3d p2)
    {
      double result = 0;
      result = Math.Sqrt(Math.Pow((p2.x - p1.x), 2) + Math.Pow((p2.y - p1.y), 2));
      return result;
    }

  }
}
