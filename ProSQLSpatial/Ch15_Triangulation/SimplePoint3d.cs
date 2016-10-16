using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProSQLSpatial
{
  public partial class Ch15_Triangulation
  {
    // Define a simple 3d point structure
    private struct SimplePoint3d : IComparable
    {
      public double x, y, z;
      public SimplePoint3d(double x, double y, double z)
      {
        this.x = x;
        this.y = y;
        this.z = z;
      }
      // Implement IComparable CompareTo method to enable sorting
      int IComparable.CompareTo(object obj)
      {
        SimplePoint3d other = (SimplePoint3d)obj;
        if (this.x > other.x) { return 1; }
        else if (this.x < other.x) { return -1; }
        else { return 0; }
      }
    }

  }
}
