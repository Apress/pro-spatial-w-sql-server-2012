using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using System.Collections.Generic;

namespace ProSQLSpatial
{
  public partial class StoredProcedures
  {

    // Declare a simple point structure
    public class kPoint
    {
      public double x, y;
      public kPoint(double x, double y)
      {
        this.x = x;
        this.y = y;
      }
      public kPoint()
      {
        this.x = double.NaN;
        this.y = double.NaN;
      }
    }
    // Define a cluster class
    public class kCluster
    {
      public kPoint Centroid;
      public List<kPoint> Points;

      public kCluster()
      {
        this.Centroid = new kPoint();
        this.Points = new List<kPoint>();
      }
      public List<kPoint> GetPoints()
      {
        return this.Points;
      }
      public kPoint GetCentroid()
      {
        return this.Centroid;
      }
      public void SetCentroid(kPoint p)
      {
        this.Centroid = p;
      }
      public void AddPoint(kPoint p)
      {
        this.Points.Add(p);
        this.RecalculateCentroid();
      }
      public void RemovePoint(kPoint p)
      {
        this.Points.Remove(p);
        this.RecalculateCentroid();
      }
      public int NumPoints()
      {
        return this.Points.Count;
      }
      public kPoint PointN(int n)
      {
        return this.Points[n];
      }
      public void RecalculateCentroid()
      {
        double n = (double)this.NumPoints();
        if (n > 0)
        {
          double avgx = (from p in this.Points select p.x).Sum() / n;
          double avgy = (from p in this.Points select p.y).Sum() / n;
          this.Centroid = new kPoint(avgx, avgy);
        }
      }
    }

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void kGeometryMeans(SqlGeometry MultiPoint, int k)
    {
      // Check that we aren't creating more clusters than points
      if (MultiPoint.STNumPoints() < k)
      {
        throw new Exception("Number of clusters cannot be greater than number of points");
      }

      /**
       * 1.) Initialisation Step
       */

      // Create k empty clusters
      List<kCluster> Clusters = new List<kCluster>();
      for (int c = 0; c < k; c++)
      {
        // Each cluster starts as an empty collection centred at (0, 0) 
        Clusters.Add(new kCluster());
      }
      // Assign each point to an arbitrary initial cluster
      int C = 0;
      for (int n = 1; n <= MultiPoint.STNumPoints(); n++)
      {
        kPoint p = new kPoint((double)MultiPoint.STPointN(n).STX, (double)MultiPoint.STPointN(n).STY);
        Clusters[C].AddPoint(p);
        C++;
        if (C >= Clusters.Count) { C = 0; }
      }
    
      // Print some debug information
      SqlContext.Pipe.Send("There are " + Clusters.Count + " clusters, containing a total of " + MultiPoint.STNumPoints() + " points.");


      /**
       * 2.) Assignment Step
       * Loop through every point and assign them to their closest cluster
       */

      // This variable will keep track of when to break the loop
      bool convergancereached = false;

      while (!convergancereached)
      {
        // On each iteration, assume points won't move clusters
        convergancereached = true;

        // Loop through every cluster
        for (int c = 0; c < k; c++)
        {
          // Loop through every point in this cluster
          for (int pointIndex = 0; pointIndex < Clusters[c].NumPoints(); pointIndex++)
          {
            // Retrieve this point 
            kPoint Point = Clusters[c].PointN(pointIndex);

            // Determine the closest cluster for this point
            int nearestCluster = GetNearestCluster(Point, Clusters);

            // If this is not the cluster in which the point currently lies...
            if (nearestCluster != c)
            {
              // Add the point to its nearest cluster
              Clusters[nearestCluster].AddPoint(Point);
              // And remove it from its previous cluster
              Clusters[c].RemovePoint(Point);
              // A point has changed clusters, so we need to continue iterating
              convergancereached = false;
            }
          }
        }

        /**
         * 3.) Update Step
         * Compute the new centres of each cluster
         */
       // foreach(Cluster c in Clusters)
        for (int c = 0; c < Clusters.Count; c++)
        {
          Clusters[c].RecalculateCentroid();
        }
      }

      /**
       * OUTPUT
       */
      // Use the first record to set the SRID
      int srid = (int)MultiPoint.STSrid;

      SqlMetaData[] columns = new SqlMetaData[3];
      columns[0] = new SqlMetaData("ClusterID", SqlDbType.Int);
      columns[1] = new SqlMetaData("Centroid", SqlDbType.Udt, typeof(SqlGeometry));
      columns[2] = new SqlMetaData("Points", SqlDbType.Udt, typeof(SqlGeometry));

      // Create a record object that represents an individual row, including it's metadata.
      SqlDataRecord record = new SqlDataRecord(columns);

      SqlContext.Pipe.SendResultsStart(record);
      for (int c = 0; c < Clusters.Count; c++)
      {
        record.SetValue(0, c);
        SqlGeometry Centroid = SqlGeometry.Point(Clusters[c].GetCentroid().x, Clusters[c].GetCentroid().y, srid);
        record.SetValue(1, Centroid);

        SqlGeometry Points = SqlGeometry.STGeomFromText(new SqlChars("GEOMETRYCOLLECTION EMPTY"), srid);
        foreach (kPoint p in Clusters[c].GetPoints())
        {
          Points = Points.STUnion(SqlGeometry.Point(p.x, p.y, srid));
        } 
        record.SetValue(2, Points);
        SqlContext.Pipe.SendResultsRow(record);
      }
      SqlContext.Pipe.SendResultsEnd();
    }

    /**
     * Get the index of the closest cluster to a given point
     */
    public static int GetNearestCluster(kPoint p, List<kCluster> Clusters)
    {
      double minDistance = double.MaxValue;
      int nearestClusterIndex = -1;

      for (int x = 0; x < Clusters.Count; x++)
      {
        // Calculate the distance to the current cluster
        double distance = Math.Sqrt(Math.Pow(p.x - Clusters[x].Centroid.x, 2.0) + Math.Pow(p.y - Clusters[x].Centroid.y, 2.0));

        // If this Cluster is closer than the previous closest
        if (distance < minDistance)
        {
          // Set this cluster as the closest
          nearestClusterIndex = x;
          minDistance = distance;
        }
      }
      return nearestClusterIndex;
    }
  }
}