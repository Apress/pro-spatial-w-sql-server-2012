using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;

namespace ProSQLSpatial.Ch14
{
  public partial class UserDefinedFunctions
  {
     [Microsoft.SqlServer.Server.SqlFunction()]
     public static SqlGeometry Extend(
       SqlGeometry @geom1, 
       SqlGeometry @geom2, 
       SqlInt32 @Offset)
     {
       // Start the LineString
       SqlGeometryBuilder gb = new SqlGeometryBuilder();
       gb.SetSrid((int)(@geom1.STSrid));
       gb.BeginGeometry(OpenGisGeometryType.LineString);
       gb.BeginFigure(
         (double)@geom1.STStartPoint().STX,
         (double)@geom1.STStartPoint().STY
       );
       // Add the points from the first geometry
       for (int x = 2; x <= (int)@geom1.STNumPoints(); x++) {
         gb.AddLine(
           (double)@geom1.STPointN(x).STX,
           (double)@geom1.STPointN(x).STY
         );
       }
       // Add the points from the second geometry
       for (int x = 1 + (int)@Offset; x <= (int)@geom2.STNumPoints(); x++) {
         gb.AddLine(
           (double)@geom2.STPointN(x).STX,
           (double)@geom2.STPointN(x).STY
         );
       }
       gb.EndFigure();
       gb.EndGeometry();
       return gb.ConstructedGeometry;
    }
  }
}
