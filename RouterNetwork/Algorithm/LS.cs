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
                int newCost = other.Cost + CostConfiguration.Cost(Id,other.Id);
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
        private IEnumerable<LSNode> Nodes;
        private IEnumerable<int> graph;
        private object nodesLock = new object();

        private int BaseCost(int id)
        {
            var node = Table.Nodes.FirstOrDefault(x => x.Id == id);
            if(node == null)
            {
                return int.MaxValue;
            }
            else
            {
                return node.Cost;
            }
        }

        public override void CreateRoutingTable()
        {
            var processedNodes = new HashSet<int>(Ports);
            Nodes = graph.Select(x => new LSNode()
                {
                    Id = x,
                    Cost = BaseCost(x),
                    Path = -1,
                    Parent = this
                });
            int numberOfNodes = Nodes.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                Console.WriteLine(processedNodes.Count);
                var w = Nodes.Where(n => !processedNodes.Contains(n.Id)).Min();

                foreach (var node in Nodes.Where(v => v.Id != w.Id))
                {
                    w.BuildPath(node);
                }

                Console.WriteLine(String.Join(",", Nodes));
                processedNodes.Add(w.Id);
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
            Console.WriteLine("We got a message");
            if (Ports.Contains(message.Receiver))
            {
                Console.WriteLine("Hey it's for us");
            }
            int whereWeWantToGo = message.Receiver;
            
            int nextStep = GetRoute(whereWeWantToGo);
            Console.WriteLine("Next Step: {0}",nextStep);
            SendMessage(new MessageArgs()
            {
                Data = message.Data,ExpectResponse = false,Header = message.Header,
                NextPoint = nextStep,Receiver = whereWeWantToGo
            });

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
