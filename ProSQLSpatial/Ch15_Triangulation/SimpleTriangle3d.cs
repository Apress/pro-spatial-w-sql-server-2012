using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProSQLSpatial
{
  public partial class Ch15_Triangulation
  {

    // Declare a simple triangle struct
    private struct SimpleTriangle3d
    {
      // Index entries to each vertex
      public int a, b, c;
      // x, y of the centre, and radius of the circumcircle
      public SimplePoint3d circumcentre;
      public double radius;
      public SimpleTriangle3d(int a, int b, int c, SimplePoint3d circumcentre, double radius)
      {
        this.a = a;
        this.b = b;
        this.c = c;
        this.circumcentre = circumcentre;
        this.radius = radius;
      }
    }
  }
}
