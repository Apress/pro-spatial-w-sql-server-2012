using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;
using System.Data.SqlClient; // Required for context connection
using Microsoft.SqlServer.Server; // SqlFunction Decoration

using ProjNet.CoordinateSystems;
using ProjNet.Converters;

namespace Ch8_Transformation
{
  public partial class UserDefinedFunctions
  {

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlGeometry GeometryToGeometry(SqlGeometry geom, SqlInt32 toSRID)
    {
      using (SqlConnection conn = new SqlConnection("context connection=true"))
      {
        conn.Open();

        // Retrieve the parameters of the source spatial reference system
        SqlCommand cmd = new SqlCommand("SELECT well_known_text FROM prospatial_reference_systems WHERE spatial_reference_id = @srid", conn);
        cmd.Parameters.Add(new SqlParameter("srid", geom.STSrid));
        String fromWKT = (String)cmd.ExecuteScalar();

        // Create the source coordinate system from WKT
        ICoordinateSystem fromCS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(fromWKT) as ICoordinateSystem;

        // Retrieve the parameters of the destination spatial reference system
        cmd.Parameters["srid"].Value = toSRID;
        String toWKT = (String)cmd.ExecuteScalar();
        cmd.Dispose();

        // Create the destination coordinate system from WKT
        ICoordinateSystem toCS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(toWKT) as ICoordinateSystem;

        // Create a CoordinateTransformationFactory:
        ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

        // Create the transformation instance:
        ProjNet.CoordinateSystems.Transformations.ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(fromCS, toCS);

        // create a sink that will create a geometry instance
        SqlGeometryBuilder b = new SqlGeometryBuilder();
        b.SetSrid((int)toSRID);

        // create a sink to do the shift and plug it in to the builder
        TransformGeometryToGeometrySink s = new TransformGeometryToGeometrySink(trans, b);

        // plug our sink into the geometry instance and run the pipeline
        geom.Populate(s);

        // the end of our pipeline is now populated with the shifted geometry instance
        return b.ConstructedGeometry;
      }
    }
  }
}
