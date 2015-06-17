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
            public int Path { get; set; }
            public LSSender Parent { private get; set; }
            public void BuildPath(LSNode other)
            {
                int newCost = other.Cost + Parent.Cost(this.Id, other.Id);
                if (IsAdjacent(other) && newCost < this.Cost)
                {
                    Path = other.Id;
                    this.Cost = newCost;
                }
            }
        }

        public LSSender(AdjacencyTable table, int[] ports, IEnumerable<int> graph)
            : base(table, ports)
        {
            this.graph = graph;
        }

        int Cost<T>(T a, T b)
        {
            /*
            //var node = graph.First(x => x.Id == a).Nodes.FirstOrDefault(x => x.Id == b);
            if (node == null)
            {
                return int.MaxValue;
            }

            int cost = node.Cost;
            if (cost != int.MaxValue)
            {
                return cost;
            }
            else
            {
                /*var message =  SendMessageWithAnswer(new MessageArgs()
                    {
                        Receiver = b
                    });
                int costAB = 0;
                node.Cost = costAB;
                return node.Cost;*/
            return 0;
            
        }
        private IEnumerable<LSNode> Nodes;
        private IEnumerable<int> graph;
        private object nodesLock = new object();
        public override void CreateRoutingTable()
        {
            var processedNodes = new HashSet<LSNode>();
            Nodes = graph.Select(x => new LSNode()
                {
                    Id = x,
                    Cost = int.MaxValue,
                    Path = -1,
                    Parent = this
                });
            int numberOfNodes = Nodes.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                Console.WriteLine(processedNodes.Count);
                var w = Nodes.Where(n => !processedNodes.Contains(n)).Min();

                foreach (var node in Nodes.Where(v => v.Id != w.Id))
                {
                    w.BuildPath(node);
                }

                Console.WriteLine(String.Join(",", Nodes));
                processedNodes.Add(w);
            }
        }
        private IEnumerable<AdjacencyTable> CreateGlobalAdjacencyTable(IEnumerable<RoutingNode> adjacentNodes)
        {
            foreach (var node in adjacentNodes)
            {
                SendMessage(new MessageArgs()
                    {
                        Receiver = node.Id
                    });
                yield return new AdjacencyTable(adjacentNodes, Ports.ToArray());
            }
        }


        protected override int GetRoute(int id)
        {
            lock (nodesLock)
            {
                return Nodes.First(x => x.Id == id).Path;
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
