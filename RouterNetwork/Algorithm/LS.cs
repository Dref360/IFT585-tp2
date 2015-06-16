using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class LSSender : RoutingSender
    {
        private class LSNode : RoutingNode
        {
            public Guid OldRoute { get; set; }
            public void BuildPath(LSNode other)
            {
                int newCost = other.Cost + Cost(this.RouterId, other.RouterId);
                if (IsAdjacent(this.RouterId, other.RouterId) && newCost < this.Cost)
                {
                    OldRoute = other.RouterId;
                    this.Cost = newCost;
                }
            }
        }

        public LSSender(params int[] ports)
            : base(ports)
        {

        }

        static bool IsAdjacent(Guid a, Guid b)
        {
            return true;
        }
        static int Cost(Guid a, Guid b)
        {
            return 0;
        }
        private IEnumerable<LSNode> Nodes;
        private List<AdjacencyTable> graph;
        private object nodesLock = new object();
        public override void CreateRoutingTable(AdjacencyTable table)
        {
            var processedNodes = new HashSet<LSNode>();
            Nodes = table.Nodes.Cast<LSNode>();
            int numberOfNodes = Nodes.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                var w = Nodes.Where(n => !processedNodes.Contains(n)).Min();
                lock (nodesLock)
                {
                    foreach (var node in Nodes.Where(v => v != w))
                    {
                        w.BuildPath(node);
                    }
                }
                processedNodes.Add(w);
            }
        }
        private IEnumerable<AdjacencyTable> CreateGlobalAdjacencyTable(IEnumerable<RoutingNode> adjacentNodes)
        {
            yield return new AdjacencyTable(new Guid(),Enumerable.Empty<RoutingNode>());
        }


        protected override Guid GetRoute(Guid id)
        {
            lock (nodesLock)
            {
                return Nodes.First(x => x.RouterId == id).OldRoute;
            }
        }

        public override void HandleRoutingRequests(MessageArgs message)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, graph);
                SendMessage(new MessageArgs()
                {
                    Data = ms.ToArray(),
                    Receiver = message.Receiver,//Sender
                });
            }
        }
    }
}
