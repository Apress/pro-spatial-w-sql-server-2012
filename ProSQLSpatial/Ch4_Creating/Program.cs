using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;

namespace Ch4_Creating
{
  class Program
  {
    static void Main(string[] args)
    {
      // EXAMPLE 1 : POINT

      // Create a new instance of the SqlGeographyBuilder
      SqlGeometryBuilder gb = new SqlGeometryBuilder();

      // Set the spatial reference identifier
      gb.SetSrid(27700);

      // Declare the type of geometry to be created
      gb.BeginGeometry(OpenGisGeometryType.Point);

      // Add the coordinates of the first (and only) point
      gb.BeginFigure(300500, 600200);

      // End the figure
      gb.EndFigure();

      // End the geometry
      gb.EndGeometry();

      // Retrieve the constructed geometry
      SqlGeometry Point = gb.ConstructedGeometry;

      // Print WKT of the geometry to the console window
      Console.WriteLine(Point.ToString());




      // EXAMPLE 2 : MULTIPOINT

      // Create a new instance of the SqlGeographyBuilder
      SqlGeographyBuilder gb2 = new SqlGeographyBuilder();

      // Set the spatial reference identifier
      gb2.SetSrid(4269);

      // Declare the type of collection to be created
      gb2.BeginGeography(OpenGisGeographyType.MultiPoint);

      // Create the first point in the collection
      gb2.BeginGeography(OpenGisGeographyType.Point);
      gb2.BeginFigure(40, -120);
      gb2.EndFigure();
      gb2.EndGeography();

      // Create the second point in the collection
      gb2.BeginGeography(OpenGisGeographyType.Point);
      gb2.BeginFigure(45, -100);
      gb2.EndFigure();
      gb2.EndGeography();

      // Create the second point in the collection
      gb2.BeginGeography(OpenGisGeographyType.Point);
      gb2.BeginFigure(42, -110);
      gb2.EndFigure();
      gb2.EndGeography();

      // End the geometry and retrieve the constructed instance
      gb2.EndGeography();
      SqlGeography MultiPoint = gb2.ConstructedGeography;
      Console.WriteLine(MultiPoint.ToString());



      // EXAMPLE 3: POLYGON WITH INTERIOR RING
      // Create a new instance of the SqlGeometryBuilder
      SqlGeometryBuilder gb3 = new SqlGeometryBuilder();

      // Set the spatial reference identifier
      gb3.SetSrid(0);

      // Declare the type of geometry to be created
      gb3.BeginGeometry(OpenGisGeometryType.Polygon);

      // Exterior ring
      gb3.BeginFigure(0, 0);
      gb3.AddLine(10, 0);
      gb3.AddLine(10, 20);
      gb3.AddLine(0, 20);
      gb3.AddLine(0, 0);
      gb3.EndFigure();

      // Interior ring
      gb3.BeginFigure(3, 3);
      gb3.AddLine(7, 3);
      gb3.AddLine(5, 17);
      gb3.AddLine(3, 3);
      gb3.EndFigure();

      // End the geometry and retrieve the constructed instance
      gb3.EndGeometry();
      SqlGeometry Polygon = gb3.ConstructedGeometry;



      // EXAMPLE 4: CURVED GEOMETRIES

      // Create a new instance of the SqlGeographyBuilder
      SqlGeometryBuilder gb4 = new SqlGeometryBuilder();

      // Set the spatial reference identifier
      gb4.SetSrid(0);

      // Declare the type of geometry to be created
      gb4.BeginGeometry(OpenGisGeometryType.CompoundCurve);

      // Begin the figure at a point
      gb4.BeginFigure(50, 0);

      // Draw a straight line edge
      gb4.AddLine(50, 10);

      // Draw a circular curved edge
      gb4.AddCircularArc(55, 5, 60, 0);
      
      // End the figure
      gb4.EndFigure();

      // End the geometry
      gb4.EndGeometry();

      SqlGeometry CompoundCurve = gb4.ConstructedGeometry;
      Console.WriteLine(CompoundCurve.ToString());



      // EXAMPLE 5: 3- AND 4- DIMENSIONAL COORDINATES

      // Create a new instance of the SqlGeographyBuilder
      SqlGeographyBuilder gb5 = new SqlGeographyBuilder();

      // Set the spatial reference identifier
      gb5.SetSrid(4326);

      // Declare the type of collection to be created
      gb5.BeginGeography(OpenGisGeographyType.Point);
      gb5.BeginFigure(52, 0.15, 140, null);
      gb5.EndFigure();
      gb5.EndGeography();

      SqlGeography PointZ = gb5.ConstructedGeography;
      Console.WriteLine(PointZ.ToString());

      Console.ReadLine();
    }
  }
}
