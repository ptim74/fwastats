using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.GraphViewModels
{
    public class GraphNetwork
    {
        public List<GraphNode> Nodes { get; set; }
        public List<GraphEdge> Edges { get; set; }

        public GraphNetwork()
        {
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
        }
    }
}
