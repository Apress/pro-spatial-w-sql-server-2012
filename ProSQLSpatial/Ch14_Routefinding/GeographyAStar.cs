using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using System.Collections.Generic;

namespace ProSQLSpatial.Ch14
{
  public partial class StoredProcedures
  {

    // Declare a class to represent each node
    private class AStarNode : IComparable
    {
      public int NodeID;
      public int ParentID;
      public double f; // the total estimated cost of reaching the goal through this node
      public double g; // the cost of the route so far from the starting point to this node
      public double h; // the estimated remaining cost from this point to the destination route

      // Constructor
      public AStarNode(int NodeID, int ParentID, double g, double h)
      {
        this.NodeID = NodeID;
        this.ParentID = ParentID;
        this.f = g + h;
        this.g = g;
        this.h = h;
      }

      // Implement the iComparable interface sort nodes on the open list
      // by ascending f score
      int IComparable.CompareTo(object obj)
      {
        AStarNode other = (AStarNode)obj;
        if (this.f < other.f)
          return -1;
        else if (this.f > other.f)
          return 1;
        else
          return 0;
      }
    }

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void GeographyAStar(SqlInt32 StartID, SqlInt32 GoalID)
    {

      /**
       * INITIALISATION
       */
      // The "Open List" contains the nodes that have yet to be assessed
      List<AStarNode> OpenList = new List<AStarNode>();

      // The "Closed List" contains the nodes that have already been assessed
      // Implemented as a Dictionary<> to enable quick lookup of nodes
      Dictionary<int, AStarNode> ClosedList = new Dictionary<int, AStarNode>();

      using (SqlConnection conn = new SqlConnection("context connection=true;"))
      {
        conn.Open();

        // Retrieve the location of the StartID
        SqlCommand cmdGetStartNode = new SqlCommand("SELECT geog4326 FROM Nodes WHERE NodeID = @id", conn);
        SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
        param.Value = StartID;
        cmdGetStartNode.Parameters.Add(param);
        object startNode = cmdGetStartNode.ExecuteScalar();
        SqlGeography startGeom;
        if (startNode != null)
        { startGeom = (SqlGeography)(startNode); }
        else
        {
          throw new Exception("Couldn't find start node with ID " + StartID.ToString());
        }
        cmdGetStartNode.Dispose();

        // Retrieve the location of the GoalID;
        SqlCommand cmdGetEndNode = new SqlCommand("SELECT geog4326 FROM Nodes WHERE NodeID = @id", conn);
        SqlParameter endparam = new SqlParameter("@id", SqlDbType.Int);
        endparam.Value = GoalID;
        cmdGetEndNode.Parameters.Add(endparam);
        object endNode = cmdGetEndNode.ExecuteScalar();
        SqlGeography endGeom;
        if (endNode != null)
        { endGeom = (SqlGeography)(endNode); }
        else
        {
          throw new Exception("Couldn't find end node with ID " + GoalID.ToString());
        }
        cmdGetEndNode.Dispose();
        conn.Close();

        // To start with, the only point we know about is the start node
        AStarNode StartNode = new AStarNode(
          (int)StartID, // ID of this node
          -1, // Start node has no parent
          0, // g - the distance travelled so far to get to this node
          (double)startGeom.STDistance(endGeom) // h - the estimated remaining distance to the goal
        );

        // Add the start node to the open list
        OpenList.Add(StartNode);

        /**
         * TRAVERSAL THROUGH THE NETWORK
         */

        // So long as there are open nodes to assess
        while (OpenList.Count > 0)
        {

          // Sort the list of open nodes by ascending f score
          OpenList.Sort(delegate(AStarNode p1, AStarNode p2)
          { return p1.f.CompareTo(p2.f); });

          // Consider the open node with lowest f score
          AStarNode NodeCurrent = OpenList[0];

          /**
           * GOAL FOUND
           */
          if (NodeCurrent.NodeID == GoalID)
          {

            // Reconstruct the route to get here
            List<SqlGeography> route = new List<SqlGeography>();
            int parentID = NodeCurrent.ParentID;

            // Keep looking back through nodes until we get to the start (parent -1)
            while (parentID != -1)
            {
              conn.Open();

              SqlCommand cmdSelectEdge = new SqlCommand("GetEdgeBetweenNodes", conn);
              cmdSelectEdge.CommandType = CommandType.StoredProcedure;
              SqlParameter fromOSODRparam = new SqlParameter("@NodeID1", SqlDbType.Int);
              SqlParameter toOSODRparam = new SqlParameter("@NodeID2", SqlDbType.Int);
              fromOSODRparam.Value = NodeCurrent.ParentID;
              toOSODRparam.Value = NodeCurrent.NodeID;
              cmdSelectEdge.Parameters.Add(fromOSODRparam);
              cmdSelectEdge.Parameters.Add(toOSODRparam);

              object edge = cmdSelectEdge.ExecuteScalar();
              SqlGeography edgeGeom;
              if (edge != null)
              {
                edgeGeom = (SqlGeography)(edge);
                route.Add(edgeGeom);
              }
              conn.Close();

              NodeCurrent = ClosedList[parentID];
              parentID = NodeCurrent.ParentID;
            }

            // Send the results back to the client
            SqlMetaData ResultMetaData = new SqlMetaData("Route", SqlDbType.Udt, typeof(SqlGeography));
            SqlDataRecord Record = new SqlDataRecord(ResultMetaData);
            SqlContext.Pipe.SendResultsStart(Record);
            // Loop through route segments in reverse order
            for (int k = route.Count - 1; k >= 0; k--)
            {
              Record.SetValue(0, route[k]);
              SqlContext.Pipe.SendResultsRow(Record);
            }
            SqlContext.Pipe.SendResultsEnd();

            return;
          } // End if (NodeCurrent.NodeID == GoalID)

          /**
           * GOAL NOT YET FOUND - IDENTIFY ALL NODES ACCESSIBLE FROM CURRENT NODE
           */
          List<AStarNode> Successors = new List<AStarNode>();
          conn.Open();
          SqlCommand cmdSelectSuccessors = new SqlCommand("GetNodesAccessibleFromNode", conn);
          cmdSelectSuccessors.CommandType = CommandType.StoredProcedure;
          SqlParameter CurrentNodeOSODRparam = new SqlParameter("@NodeID", SqlDbType.Int);
          CurrentNodeOSODRparam.Value = NodeCurrent.NodeID;
          cmdSelectSuccessors.Parameters.Add(CurrentNodeOSODRparam);

          using (SqlDataReader dr = cmdSelectSuccessors.ExecuteReader())
          {
            while (dr.Read())
            {
              // Create a node for this potential successor   
              AStarNode SuccessorNode = new AStarNode(
                dr.GetInt32(0), // NodeID
                NodeCurrent.NodeID, // Successor node is a child of the current node
                NodeCurrent.g + dr.GetDouble(1), // Additional distance from current node to successor
               (double)(((SqlGeography)dr.GetValue(2)).STDistance(endGeom))
              );
              // Add the end of the list of successors
              Successors.Add(SuccessorNode);
            }
          }
          cmdSelectSuccessors.Dispose();
          conn.Close();

          /**
           * Examine list of possible nodes to go next
           */
          SqlContext.Pipe.Send("Possible nodes to visit from " + NodeCurrent.NodeID.ToString());
          foreach (AStarNode NodeSuccessor in Successors)
          {
            // Keep track of whether we have already found this node
            bool found = false;

            // If this node is already on the closed list, it doesn't need to be examined further
            if (ClosedList.ContainsKey(NodeSuccessor.NodeID))
            {
              found = true;
              SqlContext.Pipe.Send(NodeSuccessor.NodeID.ToString() + "(" + NodeSuccessor.f.ToString() + ") (already visited)");
            }

            // If we didn't find the node on the closed list, look for it on the open list
            if (!found)
              for (int j = 0; j < OpenList.Count; j++)
              {
                if (OpenList[j].NodeID == NodeSuccessor.NodeID)
                {
                  found = true;
                  SqlContext.Pipe.Send(NodeSuccessor.NodeID.ToString() + "(" + NodeSuccessor.f.ToString() + ") (already on list to consider)");
                  // If this is a cheaper way to get there
                  if (OpenList[j].h > NodeSuccessor.h)
                  {
                    // Update the route on the open list
                    OpenList[j] = NodeSuccessor;
                  }
                  break;
                }
              }

            // If not on either list, add to the open list
            if (!found)
            {
              OpenList.Add(NodeSuccessor);
              SqlContext.Pipe.Send(NodeSuccessor.NodeID.ToString() + "(" + NodeSuccessor.f.ToString() + ") (new)");
            }
          }
          // SqlContext.Pipe.Send("---"); 

          // Once all successors have been examined, we've finished with the current node
          // so move it to the closed list
          OpenList.Remove(NodeCurrent);
          ClosedList.Add(NodeCurrent.NodeID, NodeCurrent);

        } // end while (OpenList.Count > 0)

        SqlContext.Pipe.Send("No route could be found!");
        return;
      }
    }
  }
}