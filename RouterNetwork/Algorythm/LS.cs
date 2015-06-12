using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class LS : IRoutingAlgorithm
    {
        private class Node : IComparable<Node>
        {
            public Guid RouterId { get; set; }
            public Guid OldRoute { get; set; }
            public int Cost { get; set; }
            public override int GetHashCode()
            {
                return RouterId.GetHashCode();
            }

            public int CompareTo(Node other)
            {
                return Cost.CompareTo(other.Cost);
            }
            public Node BuildPath(Node other)
            {
                int newCost = other.Cost + Cost(this.RouterId, other.RouterId);
                if (IsAdjacent(this.RouterId,other.RouterId) && newCost < this.Cost)
                {
                    return new Node()
                    {
                        RouterId = RouterId,
                        OldRoute = other.RouterId,
                        Cost = newCost
                    };
                }
                else
                {
                    return this;
                }

            }
        }
        IEnumerable<Guid> Graph();
        static bool IsAdjacent(Guid a, Guid b);
        static int Cost(Guid a, Guid b);
        public void UpdateRoute()
        {
            var processedNodes = new HashSet<Node>();
            var nodes = new Guid();
            var nodesToProcess = Graph().Where(x => x != nodes).Select(p => new Node()
                {
                    RouterId = p,
                    Cost = IsAdjacent(nodes, p) ? Cost(nodes, p) : int.MaxValue
                });
            int numberOfNodes = nodesToProcess.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                var w = nodesToProcess.Where(n => !processedNodes.Contains(n)).Min();
                processedNodes.Add(w);
                nodesToProcess = nodesToProcess.Where(v => v != w).Select(w.BuildPath);
            }
        }

        public int GetRoute(int port)
        {
            throw new NotImplementedException();
        }

        public void UpdateRoute(byte[] readBuffer, int packetSize)
        {
            throw new NotImplementedException();
        }
    }
}
