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
        private List<LSNode> Nodes;
        private IEnumerable<int> graph;
        private object nodesLock = new object();

        private int BaseCost(int id)
        {
            var node = Table.Nodes.FirstOrDefault(x => x.Id == id);
            if(node == null)
            {
                return 1000000;
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
                }).ToList();
            int numberOfNodes = Nodes.Count();
            while (numberOfNodes > processedNodes.Count)
            {
                //Console.WriteLine(processedNodes.Count);
                var w = Nodes.Where(n => !processedNodes.Contains(n.Id)).Min();
                
                foreach (var node in Nodes.Where(v => v.Id != w.Id))
                {
                    node.BuildPath(w);
                }

                processedNodes.Add(w.Id);
                var minW = CostConfiguration.SameRouter(w.Id).Concat(new []{w.Id}).Select(p => Nodes.First(x => x.Id == p)).Min();
                foreach (var port in CostConfiguration.SameRouter(w.Id))
                {
                    foreach (var x in Nodes)
                    {
                        if(x.Id == port && x.Id != minW.Id)
                        {

                            x.Cost = minW.Cost + 1;
                            x.Path = minW.Id;
                        }
                    }
                }
            }
            Console.WriteLine(GetRoute(17009));
        }


        protected override int GetRoute(int id)
        {
            if (Ports.Any(i=>(id / 10) == (i / 10)))
            {
                return id;
            }
            try
            {
                return GetRoute(Nodes.First(x => x.Id == id).Path);
            }
            catch(Exception ex)
            {
                throw ex;
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

            return null;
        }
    }
}
