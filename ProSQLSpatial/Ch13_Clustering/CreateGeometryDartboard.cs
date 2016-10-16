using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using System.Collections.Generic;

namespace ProSQLSpatial
{
  public partial class StoredProcedures
  {
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void CreateGeometryDartboard(SqlGeometry centre, double radius, int numrings)
    {
      int srid = (int)centre.STSrid;

      List<SqlDataRecord> Grid = new List<SqlDataRecord>();
      SqlMetaData[] Columns = {
        new SqlMetaData("CellId", SqlDbType.Int),
        new SqlMetaData("Cell", SqlDbType.Udt, typeof(SqlGeometry))
      };

      for (int x = 0; x < numrings; x++)
      {
        SqlGeometry Ring = centre.STBuffer(radius*(x+1));
        SqlGeometry Hole = centre.STBuffer(radius * x);
        Ring = Ring.STDifference(Hole);
        
          SqlDataRecord rec = new SqlDataRecord(Columns);
          rec.SetInt32(0, x);
          rec.SetValue(1, Ring);
          Grid.Add(rec);
        }

      SqlContext.Pipe.SendResultsStart(new SqlDataRecord(Columns));
      foreach (SqlDataRecord d in Grid)
      {
        SqlContext.Pipe.SendResultsRow(d);
      }
      SqlContext.Pipe.SendResultsEnd();
    }
  }
}