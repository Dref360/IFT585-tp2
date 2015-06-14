using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class LS : IRoutingAlgorithm
    {
        private class LSNode : RoutingNode
        {
            public Guid OldRoute { get; set; }
            public LSNode BuildPath(LSNode other)
            {
                int newCost = other.Cost + Cost(this.RouterId, other.RouterId);
                if (IsAdjacent(this.RouterId, other.RouterId) && newCost < this.Cost)
                {
                    return new LSNode()
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
        IEnumerable<Guid> Graph()
        {
            yield return new Guid();
        }
        static bool IsAdjacent(Guid a, Guid b)
        {
            return true;
        }
        static int Cost(Guid a, Guid b)
        {
            return 0;
        }
        public void UpdateRoute()
        {
            var processedNodes = new HashSet<LSNode>();
            var nodes = new Guid();
            var nodesToProcess = Graph().Where(x => x != nodes).Select(id => new LSNode()
                {
                    RouterId = id,
                    Cost = IsAdjacent(nodes, id) ? Cost(nodes, id) : int.MaxValue
                });
            int numberOfNodes = nodesToProcess.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                var w = nodesToProcess.Where(n => !processedNodes.Contains(n)).Min();
                processedNodes.Add(w);
                nodesToProcess = nodesToProcess.Where(v => v != w).Select(w.BuildPath).Concat(new LSNode[] { w });
            }
        }

        public void CreateRoutingTable(AdjacencyTable table)
        {
            throw new NotImplementedException();
        }

        public EventHandler<MessageArgs> SendMessage { get; set; }

        public void HandleRequests(MessageArgs message)
        {
            throw new NotImplementedException();
        }


        public Guid GetRoute(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
