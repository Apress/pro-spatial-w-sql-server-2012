using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;
using System.Xml;
using Microsoft.SqlServer.Types;
using System.Collections.Generic; // Used for List<>

namespace ProSQLSpatial.Ch6
{
  public partial class UserDefinedFunctions
  {
    public static XmlDocument Geocode( 
      string countryRegion, 
      string adminDistrict,
      string locality, 
      string postalCode,
      string addressLine
    )
    {
      // Variable to hold the geocode response
      XmlDocument xmlResponse = new XmlDocument();
      // Bing Maps key used to access the Locations API service
      string key = "ENTERYOURBINGMAPSKEYHERE";
      // URI template for making a geocode request
      string urltemplate = "http://dev.virtualearth.net/REST/v1/Locations?countryRegion={0}&adminDistrict={1}&locality={2}&postalCode={3}&addressLine={4}&key={5}&output=xml";
      // Insert the supplied parameters into the URL template
      string url = string.Format(urltemplate, countryRegion, adminDistrict, locality, postalCode, addressLine, key);
      try {
        // Initialise web request
        HttpWebRequest webrequest = null;
        HttpWebResponse webresponse = null;
        Stream stream = null;
        StreamReader streamReader = null;
        // Make request to the Locations API REST service
        webrequest = (HttpWebRequest)WebRequest.Create(url);
        webrequest.Method = "GET";
        webrequest.ContentLength = 0;
        // Retrieve the response
        webresponse = (HttpWebResponse)webrequest.GetResponse();
        stream = webresponse.GetResponseStream();
        streamReader = new StreamReader(stream);
        xmlResponse.LoadXml(streamReader.ReadToEnd());
        // Clean up
        webresponse.Close();
        stream.Dispose();
        streamReader.Dispose();
      }
      catch(Exception ex)
      {
        // Exception handling code here;
      }
      // Return an XMLDocument with the geocoded results 
      return xmlResponse;
    }


    // Declare a UDF wrapper method
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlGeography GeocodeUDF(
      SqlString countryRegion, 
      SqlString adminDistrict,
      SqlString locality, 
      SqlString postalCode,
      SqlString addressLine
      )
    {
      XmlDocument geocodeResponse = new XmlDocument();
      SqlGeography point = new SqlGeography();
      try
      {
        geocodeResponse = Geocode(
          (string)countryRegion,
          (string)adminDistrict,
          (string)locality,
          (string)postalCode,
          (string)addressLine
        );
      }
      // Failed to geocode the address
      catch (Exception ex)
      {
        SqlContext.Pipe.Send(ex.Message.ToString());
      }

      XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
      nsmgr.AddNamespace("ab","http://schemas.microsoft.com/search/local/ws/rest/v1");

      // Check that we received a valid response from the geocoding server
      if (geocodeResponse.GetElementsByTagName("StatusCode")[0].InnerText != "200")
      {
        throw new Exception("Didn't get correct response from geocoding server");
      }
      // Retrieve the list of geocoded locations
      XmlNodeList Locations = geocodeResponse.GetElementsByTagName("Location");
      // Create a geography Point instance of the first matching location
      double Latitude = double.Parse(Locations[0]["Point"]["Latitude"].InnerText);
      double Longitude = double.Parse(Locations[0]["Point"]["Longitude"].InnerText);
      SqlGeography Point = SqlGeography.Point(Latitude, Longitude, 4326);
      // Return the Point to SQL Server
      return Point;
    }


    // Declare a TVF wrapper method
    [Microsoft.SqlServer.Server.SqlFunction(
      Name = "GeocodeTVF",
      FillRowMethodName = "GeocodeTVFFillRow",
      DataAccess = DataAccessKind.Read,
      TableDefinition = @"Name nvarchar(255),
                          Point geography,
                          BoundingBox geography")]
    public static System.Collections.IEnumerable GeocodeTVF(
      SqlString addressLine,
      SqlString locality,
      SqlString adminDistrict,
      SqlString postalCode,
      SqlString countryRegion
      )
    {
      XmlDocument geocodeResponse = new XmlDocument();
      try
      {
        geocodeResponse = Geocode(
          (string)countryRegion,
          (string)adminDistrict,
          (string)locality,
          (string)postalCode,
          (string)addressLine
        );
      }
      // Failed to geocode the address
      catch (Exception ex)
      {
        SqlContext.Pipe.Send(ex.Message.ToString());
      }

      // Define the default XML namespace
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
      nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/search/local/ws/rest/v1");

      // Create a set of all <Location>s in the response
      XmlNodeList Locations = geocodeResponse.GetElementsByTagName("Location");

      // Set up a list to hold results
      List<object[]> items = new List<object[]>();

      // Loop through each location in the response
      foreach (XmlNode locationNode in Locations)
      {
        // Create a new object for this result
        object[] item = new object[3];

        // Retrieve the name of this location
        string Name = locationNode["Name"].InnerText;
        item.SetValue(Name, 0);

        // Create a point for this location
        double Latitude = double.Parse(locationNode["Point"]["Latitude"].InnerText);
        double Longitude = double.Parse(locationNode["Point"]["Longitude"].InnerText);
        SqlGeography Point = SqlGeography.Point(Latitude, Longitude, 4326);
        item.SetValue(Point, 1);

        // Create a polygon for this location's bounding box
        if (locationNode.SelectSingleNode("ab:BoundingBox", nsmgr) != null)
        {
          // Retrieve the latitude/longitude extents of the box
          double BBSLatitude = double.Parse(locationNode.SelectSingleNode("ab:BoundingBox/ab:SouthLatitude", nsmgr).InnerText);
          double BBNLatitude = double.Parse(locationNode.SelectSingleNode("ab:BoundingBox/ab:NorthLatitude", nsmgr).InnerText);
          double BBWLongitude = double.Parse(locationNode.SelectSingleNode("ab:BoundingBox/ab:WestLongitude", nsmgr).InnerText);
          double BBELongitude = double.Parse(locationNode.SelectSingleNode("ab:BoundingBox/ab:EastLongitude", nsmgr).InnerText);

          // Build a geography polygon of the box
          SqlGeographyBuilder gb = new SqlGeographyBuilder();
          gb.SetSrid(4326);
          gb.BeginGeography(OpenGisGeographyType.Polygon);
          gb.BeginFigure(BBSLatitude, BBWLongitude);
          gb.AddLine(BBSLatitude, BBELongitude);
          gb.AddLine(BBNLatitude, BBELongitude);
          gb.AddLine(BBNLatitude, BBWLongitude);
          gb.AddLine(BBSLatitude, BBWLongitude);
          gb.EndFigure();
          gb.EndGeography();
          SqlGeography Polygon = gb.ConstructedGeography;
          item.SetValue(Polygon, 2);
        }
        // Add this result to the set of results
        items.Add(item);
      }
      return items;
    }

    public static void GeocodeTVFFillRow(
      object obj,
      out SqlString Name,
      out SqlGeography Point,
      out SqlGeography BoundingBox)
    {
      object[] item = (object[])obj;
      Name = (SqlString)(item[0].ToString());
      Point = (SqlGeography)item[1];
      BoundingBox = (SqlGeography)item[2];
    }


  };






}