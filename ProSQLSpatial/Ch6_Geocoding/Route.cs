using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;
using Microsoft.SqlServer.Types;
using System.Xml;

namespace ProSQLSpatial.Ch14
{
  public partial class UserDefinedFunctions
  {
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlGeography RESTRoute(SqlGeography Start, SqlGeography End, SqlString Mode)
    {
      string feedData = string.Empty;

      if (!(Start.STGeometryType() == "POINT" && Start.STSrid == 4326)) {
        throw new Exception("Route start must be a single point defined using SRID 4326");
      }
      if (!(End.STGeometryType() == "POINT" && End.STSrid == 4326)) {
        throw new Exception("Route end must be a single point defined using SRID 4326");
      }
      string travelMode = ((string)Mode).ToUpper(); 

      if (travelMode != "DRIVING" && travelMode != "WALKING")
      {
        throw new Exception("Mode of travel must be WALKING or DRIVING");
      }

      try
      {
        String key = "ENTERYOURBINGMAPSKEYHERE";
        String urltemplate = "http://dev.virtualearth.net/REST/V1/Routes/{0}?wp.0={1}&wp.1={2}&rpo=Points&optmz=distance&output=xml&key={3}";
        String Startcoords = String.Concat(Start.Lat, ",", Start.Long);
        String Endcoords = String.Concat(End.Lat, ",", End.Long);


        // Request the routepoints in the results. See http://msdn.microsoft.com/en-us/library/ff701717.aspx
        String url = String.Format(urltemplate, travelMode, Startcoords, Endcoords, key);
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        Stream stream = null;
        StreamReader streamReader = null;

        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.ContentLength = 0;
        response = (HttpWebResponse)request.GetResponse();

        stream = response.GetResponseStream();
        streamReader = new StreamReader(stream);
        feedData = streamReader.ReadToEnd();

        response.Close();
        stream.Dispose();
        streamReader.Dispose();
      }


      catch (Exception ex)
      {

        SqlContext.Pipe.Send(ex.Message.ToString());

      }

      // Process the XML response
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(feedData);

      // Define the default XML namespace
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
      nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/search/local/ws/rest/v1");


      XmlNode routePath = doc.GetElementsByTagName("RoutePath")[0];
      XmlNode line = routePath["Line"];

      // Create a set of all <Location>s in the response
      XmlNodeList Points = line.SelectNodes("ab:Point", nsmgr);

      SqlGeographyBuilder gb = new SqlGeographyBuilder();
      gb.SetSrid(4326);
      gb.BeginGeography(OpenGisGeographyType.LineString);
      gb.BeginFigure(double.Parse(Points[0]["Latitude"].InnerText), double.Parse(Points[0]["Longitude"].InnerText));

      for(int i=1; i<Points.Count; i++)
      {
        gb.AddLine(double.Parse(Points[i]["Latitude"].InnerText), double.Parse(Points[i]["Longitude"].InnerText));
      }
      
      gb.EndFigure();
      gb.EndGeography();

      return gb.ConstructedGeography;
    }
  };

}