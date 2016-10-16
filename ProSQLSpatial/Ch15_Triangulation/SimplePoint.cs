using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProSQLSpatial
{
  public partial class Ch15_Triangulation
  {

    // Define a simple point structure
    private struct SimplePoint : IComparable
    {
      public double x, y;
      public SimplePoint(double x, double y)
      {
        this.x = x;
        this.y = y;
      }
      // Implement IComparable CompareTo method to enable sorting
      int IComparable.CompareTo(object obj)
      {
        SimplePoint other = (SimplePoint)obj;
        if (this.x > other.x) { return 1; }
        else if (this.x < other.x) { return -1; }
        else { return 0; }
      }
    }
  }
}
