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

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void GeometryTriangulate3d(SqlGeometry MultiPoint)
    {
      // Retrieve the SRID
      int srid = (int)MultiPoint.STSrid;

      // Check valid input
      if (!(MultiPoint.STGeometryType() == "MULTIPOINT" && MultiPoint.STNumPoints() > 3))
      {
        throw new ArgumentException("Input must be a MultiPoint containing at least three points");
      }

      // Initialise a list of vertices
      List<SimplePoint3d> Vertices = new List<SimplePoint3d>();
      // Add all the original supplied points
      for (int i = 1; i <= MultiPoint.STNumPoints(); i++)
      {
        SqlGeometry p = MultiPoint.STPointN(i);
        SimplePoint3d Point = new SimplePoint3d((double)p.STX, (double)p.STY, p.HasZ ? (double)p.Z : 0);
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
      SimplePoint3d a = new SimplePoint3d(avgx - 2 * dmax, avgy - dmax, 0);
      SimplePoint3d b = new SimplePoint3d(avgx + 2 * dmax, avgy - dmax, 0);
      SimplePoint3d c = new SimplePoint3d(avgx, avgy + 2 * dmax, 0);

      // Add the supertriangle vertices to the end of the vertex array
      Vertices.Add(a);
      Vertices.Add(b);
      Vertices.Add(c);

      double radius;
      SimplePoint3d circumcentre;
      CalculateCircumcircle3d(a, b, c, out circumcentre, out radius);

      // Create a triangle from the vertices
      SimpleTriangle3d SuperTriangle = new SimpleTriangle3d(numPoints, numPoints + 1, numPoints + 2, circumcentre, radius);

      // Add the supertriangle to the list of triangles
      List<SimpleTriangle3d> Triangles = new List<SimpleTriangle3d>();
      Triangles.Add(SuperTriangle);

      List<SimpleTriangle3d> CompletedTriangles = new List<SimpleTriangle3d>();

      // Loop through each point
      for (int i = 0; i < numPoints; i++)
      {
        // Initialise the edge buffer
        List<int[]> Edges = new List<int[]>();

        // Loop through each triangle
        for (int j = Triangles.Count - 1; j >= 0; j--)
        {
          // If the point lies within the circumcircle of this triangle
          if (Distance3d(Triangles[j].circumcentre, Vertices[i]) < Triangles[j].radius)
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
          CalculateCircumcircle3d(Vertices[Edges[j][0]], Vertices[Edges[j][1]], Vertices[i], out circumcentre, out radius);
          SimpleTriangle3d T = new SimpleTriangle3d(Edges[j][0], Edges[j][1], i, circumcentre, radius);
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
      SqlContext.Pipe.SendResultsStart(record);
      foreach (SimpleTriangle3d Tri in CompletedTriangles)
      {
        // Check that this is a triangle formed only from vertices in the original multipoint
        // i.e. not from the vertices of the supertriangle.
        if (Tri.a < numPoints && Tri.b < numPoints && Tri.c < numPoints)
        {
          SqlGeometry triangle = Triangle3dFromPoints(Vertices[Tri.a], Vertices[Tri.b], Vertices[Tri.c], srid);
          record.SetValue(0, triangle);
          SqlContext.Pipe.SendResultsRow(record);
        }
      }
      SqlContext.Pipe.SendResultsEnd();
    }
  }
}