using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace ProSQLSpatial.Ch14
{
  public partial class UserDefinedFunctions
  {
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlGeometry GeometryTSP(SqlGeometry PlacesToVisit)
    {
      // Convert the supplied MultiPoint instance into a List<> of SqlGeometry points
      List<SqlGeometry> RemainingCities = new List<SqlGeometry>();
      // Loop and add each point to the list
      for (int i = 1; i <= PlacesToVisit.STNumGeometries(); i++)
      {
        RemainingCities.Add(PlacesToVisit.STGeometryN(i));
      }
      // Start the tour from the first city
      SqlGeometry CurrentCity = RemainingCities[0];

      // Begin the geometry
      SqlGeometryBuilder Builder = new SqlGeometryBuilder();
      Builder.SetSrid((int)PlacesToVisit.STSrid);
      Builder.BeginGeometry(OpenGisGeometryType.LineString);
      // Begin the LineString with the first point
      Builder.BeginFigure((double)CurrentCity.STX, (double)CurrentCity.STY);
      // We don't need to visit this city again
      RemainingCities.Remove(CurrentCity);

      // While there are still unvisited cities
      while (RemainingCities.Count > 0)
      {
        RemainingCities.Sort(delegate(SqlGeometry p1, SqlGeometry p2)
        { return p1.STDistance(CurrentCity).CompareTo(p2.STDistance(CurrentCity)); });

        // Move to the closest destination
        CurrentCity = RemainingCities[0];

        // Add this city to the tour route
        Builder.AddLine((double)CurrentCity.STX, (double)CurrentCity.STY);

        // Update the list of remaining cities
        RemainingCities.Remove(CurrentCity);
      }

      // End the geometry
      Builder.EndFigure();
      Builder.EndGeometry();
 
      // Return the constructed geometry
      return Builder.ConstructedGeometry;
    }
  };
}