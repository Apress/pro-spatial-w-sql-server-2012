using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data;
using System.Data.Common;
using System.Web.Script.Serialization;

namespace Ch16_VisualisationUI
{
  /// <summary>
  /// Summary description for Handler
  /// </summary>
  public class Handler : IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {

      // Define connection to SQL server
      using (SqlConnection conn = new SqlConnection(@"server=localhost\denalictp3;" + "Trusted_Connection=yes;" + "database=tempdb"))
      {
        // Open the connection
        conn.Open();
       
        // Define the stored procedure to execute
        SqlCommand cmd = new SqlCommand("dbo.uspAirportLocator", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        // Send the coordinates of the clicked point
        cmd.Parameters.Add("@Latitude", SqlDbType.Float);
        cmd.Parameters["@Latitude"].Value = context.Request.Params["lat"];
        cmd.Parameters.Add("@Longitude", SqlDbType.Float);
        cmd.Parameters["@Longitude"].Value = context.Request.Params["long"];
        cmd.Parameters.Add("@Radius", SqlDbType.Float);
        cmd.Parameters["@Radius"].Value = context.Request.Params["radius"];

        // Create a reader for the result set
        SqlDataReader rdr = cmd.ExecuteReader();
        var dataQuery = from d in rdr.Cast<DbDataRecord>()
                        select new
                        {
                          Name = (String)d["Name"],
                          City = (String)d["City"],
                          State = (String)d["State"],
                          Lat = (Double)d["Latitude"],
                          Long = (Double)d["Longitude"]
                        };

        // Serialise as JSON
        var data = dataQuery.ToArray();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        String jsonData = serializer.Serialize(data);

        // Send results back to the webpage
        context.Response.ContentType = "text/plain";
        context.Response.Write(jsonData);
      }
    }

    public bool IsReusable
    {
      get
      {
        return false;
      }
    }
  }
}