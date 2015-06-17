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
            public LSSender Parent { private get; set; }
            public void BuildPath(LSNode other)
            {
                int newCost = other.Cost + Parent.Cost(this.Port, other.Port);
                if (Parent.IsAdjacent(this.Port, other.Port) && newCost < this.Cost)
                {
                    OldRoute = other.Port;
                    this.Cost = newCost;
                }
            }
        }

        public LSSender(AdjacencyTable table, int[] ports, IEnumerable<AdjacencyTable> graph)
            : base(table, ports)
        {
            this.graph = graph;
        }

        bool IsAdjacent(int a, int b)
        {
            return true;
           // return graph.First(x => x == a).Nodes.Any(x => x.RouterId == b);
        }
        int Cost(int a, int b)
        {
            var node = graph.First(x => x.Id == a).Nodes.FirstOrDefault(x => x.RouterId == b);
            if(node == null)
            {
                return int.MaxValue;
            }

            int cost = node.Cost;
            if(cost != int.MaxValue)
            {
                return cost;
            }
            else
            {
                /*var message =  SendMessageWithAnswer(new MessageArgs()
                    {
                        Receiver = b
                    });*/
                int costAB = 0;
                node.Cost = costAB;
                return node.Cost;
            }
        }
        private IEnumerable<LSNode> Nodes;
        private IEnumerable<AdjacencyTable> graph;
        private object nodesLock = new object();
        public override void CreateRoutingTable()
        {
            var processedNodes = new HashSet<LSNode>();
            Nodes = graph.Select(x => new LSNode()
                {
                    RouterId = x.Id,
                    Cost = int.MaxValue,
                    OldRoute = new int(),
                    Parent = this
                });
            int numberOfNodes = Nodes.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                Console.WriteLine(processedNodes.Count);
                var w = Nodes.Where(n => !processedNodes.Contains(n)).Min();

                    foreach (var node in Nodes.Where(v => v.RouterId != w.RouterId))
                    {
                        w.BuildPath(node);
                }
                
                Console.WriteLine(String.Join(",",Nodes));
                processedNodes.Add(w);
            }
        }
        private IEnumerable<AdjacencyTable> CreateGlobalAdjacencyTable(IEnumerable<RoutingNode> adjacentNodes)
        {
            foreach (var node in adjacentNodes)
            {
                SendMessage(new MessageArgs()
                    {
                        Receiver = node.RouterId
                    });
                yield return new AdjacencyTable(adjacentNodes,Ports.ToArray());
            }
        }


        protected override int GetRoute(int id)
        {
            lock (nodesLock)
            {
                return Nodes.First(x => x.RouterId == id).OldRoute;
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
                    Receiver = message.Receiver,//Sender
                });
                return null;
            }
        }
    }
}
