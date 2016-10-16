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
    public static void CreateGeometryGrid(SqlGeometry boundingbox, int columns, int rows)
    {
      /**
       * Create the grid of cells
       */
      SqlGeometry Envelope = boundingbox.STEnvelope();

      // PointN(3) is the top right, and PointN(1) is bottom left
      double minX = (double)Envelope.STPointN(1).STX;
      double maxX = (double)Envelope.STPointN(3).STX;
      double minY = (double)Envelope.STPointN(1).STY;
      double maxY = (double)Envelope.STPointN(3).STY;

      int srid = (int)boundingbox.STSrid;

      // Work out the height and width of the full grid
      double gridwidth = maxX - minX;
      double gridheight = maxY - minY;

      // And then calculate the width ane height of each individual cell
      double cellwidth = gridwidth / columns;
      double cellheight = gridheight / rows;

      List<SqlDataRecord> Grid = new List<SqlDataRecord>();
      SqlMetaData[] Columns = {
        new SqlMetaData("CellId", SqlDbType.Int),
        new SqlMetaData("Cell", SqlDbType.Udt, typeof(SqlGeometry))
      };

      int x = 0;
      int y = 0;
      while (y < rows)
      {
        while (x < columns)
        {
          SqlGeometryBuilder gb = new SqlGeometryBuilder();
          gb.SetSrid(srid);
          gb.BeginGeometry(OpenGisGeometryType.Polygon);
          gb.BeginFigure(minX + (x * cellwidth), minY + (y * cellheight));
          gb.AddLine(minX + ((x + 1) * cellwidth), minY + (y * cellheight));
          gb.AddLine(minX + ((x + 1) * cellwidth), minY + ((y + 1) * cellheight));
          gb.AddLine(minX + (x * cellwidth), minY + ((y + 1) * cellheight));
          gb.AddLine(minX + (x * cellwidth), minY + (y * cellheight));
          gb.EndFigure();
          gb.EndGeometry();

          SqlDataRecord rec = new SqlDataRecord(Columns);
          rec.SetInt32(0, y*columns + x);
          rec.SetValue(1, gb.ConstructedGeometry);
          Grid.Add(rec);
          x++;
        }
        y++;
        x = 0;
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