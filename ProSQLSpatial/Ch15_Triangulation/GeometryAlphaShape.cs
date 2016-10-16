using System; //String and Math fcuntions
using System.Collections.Generic; // Lists
using Microsoft.SqlServer.Types; // Required for SqlGeoemtry
using System.Data.SqlTypes; //SqlString etc.
using System.Data.SqlClient; //SqlConnection etc.
using Microsoft.SqlServer.Server; //SqlMetaData and SqlDataRecord
using System.Data; //SqlDbType

namespace ProSQLSpatial
{
  public partial class Ch15_Triangulation
  {

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlGeometry GeometryAlphaShape(SqlGeometry MultiPoint, SqlDouble alpha)
    {
      // Retrieve the SRID
      int srid = (int)MultiPoint.STSrid;

      // Check valid input
      if (!(MultiPoint.STGeometryType() == "MULTIPOINT" && MultiPoint.STNumPoints() > 3))
      {
        throw new ArgumentException("Input must be a MultiPoint containing at least three points");
      }

      // Initialise a list of vertices
      List<SimplePoint> Vertices = new List<SimplePoint>();
      // Add all the original supplied points
      for (int i = 1; i <= MultiPoint.STNumPoints(); i++)
      {
        SimplePoint Point = new SimplePoint((double)MultiPoint.STPointN(i).STX, (double)MultiPoint.STPointN(i).STY);
        // MultiPoints can contain the same point twice, but this messes up Delauney
        if (!Vertices.Contains(Point))
        {
          Vertices.Add(Point);
        }
      }

      // Important - count the number of points in the array, NOT using STNumPoints of the supplied geometry, as some duplicate points
      // may have been removed
      int numPoints = Vertices.Count;

      // Important! Sort the list so that points sweep from left - right
      Vertices.Sort();

      // Calculate the "supertriangle" that encompasses the pointset
      SqlGeometry Envelope = MultiPoint.STEnvelope();
      // Width
      double dx = (double)(Envelope.STPointN(2).STX - Envelope.STPointN(1).STX);
      // Height 
      double dy = (double)(Envelope.STPointN(4).STY - Envelope.STPointN(1).STY);
      // Maximum dimension
      double dmax = (dx > dy) ? dx : dy;
      // Centre
      double avgx = (double)Envelope.STCentroid().STX;
      double avgy = (double)Envelope.STCentroid().STY;
      // Create the points at corners of the supertriangle
      SimplePoint a = new SimplePoint(avgx - 2 * dmax, avgy - dmax);
      SimplePoint b = new SimplePoint(avgx + 2 * dmax, avgy - dmax);
      SimplePoint c = new SimplePoint(avgx, avgy + 2 * dmax);

      // Add the supertriangle vertices to the end of the vertex array
      Vertices.Add(a);
      Vertices.Add(b);
      Vertices.Add(c);

      double radius;
      SimplePoint circumcentre;
      CalculateCircumcircle(a, b, c, out circumcentre, out radius);

      // Create a triangle from the vertices
      SimpleTriangle SuperTriangle = new SimpleTriangle(numPoints, numPoints + 1, numPoints + 2, circumcentre, radius);

      // Add the supertriangle to the list of triangles
      List<SimpleTriangle> Triangles = new List<SimpleTriangle>();
      Triangles.Add(SuperTriangle);

      List<SimpleTriangle> CompletedTriangles = new List<SimpleTriangle>();

      // Loop through each point
      for (int i = 0; i < numPoints; i++)
      {
        // Initialise the edge buffer
        List<int[]> Edges = new List<int[]>();

        // Loop through each triangle
        for (int j = Triangles.Count - 1; j >= 0; j--)
        {
          // If the point lies within the circumcircle of this triangle
          if (Distance(Triangles[j].circumcentre, Vertices[i]) < Triangles[j].radius)
          {
            // Add the triangle edges to the edge buffer
            Edges.Add(new int[] {Triangles[j].a, Triangles[j].b});
            Edges.Add(new int[] { Triangles[j].b, Triangles[j].c });
            Edges.Add(new int[] { Triangles[j].c, Triangles[j].a });
            
            // Remove this triangle from the list
            Triangles.RemoveAt(j);
          }

          // If this triangle is complete
          else if (Vertices[i].x > Triangles[j].circumcentre.x + Triangles[j].radius)
          {
            {
              CompletedTriangles.Add(Triangles[j]);
            }
            Triangles.RemoveAt(j);
          }
          
        }

        // Remove duplicate edges
        for (int j = Edges.Count - 1; j > 0; j--)
        {
          for (int k = j - 1; k >= 0; k--)
          {
            // Compare if this edge match in either direction
            if (Edges[j][0].Equals(Edges[k][1]) && Edges[j][1].Equals(Edges[k][0]))
            {
              // Remove both duplicates
              Edges.RemoveAt(j);
              Edges.RemoveAt(k);

             // We've removed an item from lower down the list than where j is now, so update j
              j--;
              break;
            }
          }
        }

        // Create new triangles for the current point
        for (int j = 0; j < Edges.Count; j++)
        {
          CalculateCircumcircle(Vertices[Edges[j][0]], Vertices[Edges[j][1]], Vertices[i], out circumcentre, out radius);
          SimpleTriangle T = new SimpleTriangle(Edges[j][0], Edges[j][1], i, circumcentre, radius);
          Triangles.Add(T);
        }
      }

      // We've finished triangulation. Move any remaining triangles onto the completed list
      CompletedTriangles.AddRange(Triangles);

      // Define the metadata of the results column
      SqlMetaData metadata = new SqlMetaData("Triangle", SqlDbType.Udt, typeof(SqlGeometry));

      // Create a record based on this metadata
      SqlDataRecord record = new SqlDataRecord(metadata);

      // Send the results back to the client
      SqlGeometry result = new SqlGeometry();
      result.STSrid = srid;
      foreach (SimpleTriangle Tri in CompletedTriangles)
      {
        // Only include triangles whose radius is less than supplied alpha
        if (Tri.radius < alpha)
        {
          SqlGeometry triangle = TriangleFromPoints(Vertices[Tri.a], Vertices[Tri.b], Vertices[Tri.c], srid);
          // Create union of all matching triangles 
          result = result.STUnion(triangle);
        }
      }
      return result;
    }





  }
}