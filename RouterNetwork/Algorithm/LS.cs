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
            public int OldRoute { get; set; }
            public void BuildPath(LSNode other,LSSender sender)
            {
                int newCost = other.Cost + sender.Cost(this.Port, other.Port);
                if (IsAdjacent(this.Port, other.Port) && newCost < this.Cost)
                {
                    OldRoute = other.Port;
                    this.Cost = newCost;
                }
            }
        }

        public LSSender(AdjacencyTable table, int[] ports, IEnumerable<AdjacencyTable> graph)
            : base(table, ports)
        {
            this.graph = graph.ToList();
        }

        static bool IsAdjacent(int a, int b)
        {
            return true;
        }
        int Cost(int a, int b)
        {
            //Si on veut le chemin vers le meme routeur
            if (Ports.Contains(a) && Ports.Contains(b))
                return 0;
            return 0;
        }
        private IEnumerable<LSNode> Nodes;
        private List<AdjacencyTable> graph;
        private object nodesLock = new object();
        public override void CreateRoutingTable()
        {
            var processedNodes = new HashSet<LSNode>();
            Nodes = Table.Nodes.Cast<LSNode>();
            int numberOfNodes = Nodes.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                var w = Nodes.Where(n => !processedNodes.Contains(n)).Min();
                lock (nodesLock)
                {
                    foreach (var node in Nodes.Where(v => v != w))
                    {
                        w.BuildPath(node,this);
                    }
                }
                processedNodes.Add(w);
            }
        }
        private IEnumerable<AdjacencyTable> CreateGlobalAdjacencyTable(IEnumerable<RoutingNode> adjacentNodes)
        {
            foreach (var node in adjacentNodes)
            {
                SendMessage(new MessageArgs()
                    {
                        Receiver = node.Port
                    });
                yield return new AdjacencyTable(adjacentNodes);
            }
        }


        protected override int GetRoute(int port)
        {
            lock (nodesLock)
            {
                return Nodes.First(x => x.Port == port).OldRoute;
            }
        }

        public override MessageArgs HandleRoutingRequests(MessageArgs message)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, graph);
                SendMessage(new MessageArgs()
                {
                    Data = ms.ToArray(),
                    Receiver = message.Receiver,
                    NextPoint = GetRoute(message.Receiver)//Sender
                });
                return null;
            }
        }
    }
}
