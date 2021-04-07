using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.GraphViewModels
{
    public class GraphNetwork
    {
        public ICollection<GraphNode> Nodes { get; set; }
        public ICollection<GraphEdge> Edges { get; set; }

        public GraphNetwork()
        {
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
        }
    }
}
