﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bonjour, quel algorithme voulez-vous utiliser?\n\t 1:LS\n\t 2:DV");
            int algoType = int.Parse(Console.ReadLine());
            //var guidA = Guid.NewGuid();
            //var guidB = Guid.NewGuid();
            //var guidC = Guid.NewGuid();
            //var guidD = Guid.NewGuid();
            //var guidE = Guid.NewGuid();
            //var guidF = Guid.NewGuid();
            var tables = new[] 
            {
                new AdjacencyTable(new[] { new RoutingNode(15009, 5), new RoutingNode(18009, 45),new RoutingNode(20009,1)},15001,18001,20001),
                new AdjacencyTable(new[] { new RoutingNode(15001, 5), new RoutingNode(16009, 70), new RoutingNode(11009, 3) },15009, 16001, 11001 ),
                new AdjacencyTable(new[] { new RoutingNode(16001, 70), new RoutingNode(12001, 50), new RoutingNode(17009, 78) },16009, 12009, 17001 ),
                new AdjacencyTable(new[] { new RoutingNode(18001, 45), new RoutingNode(12009, 50), new RoutingNode(19009, 8) },18009, 12001, 19001),
                new AdjacencyTable(new[] { new RoutingNode(11001, 3), new RoutingNode(19001, 8), new RoutingNode(10009, 7) },11009, 19009, 10001 ),
                new AdjacencyTable(new[] { new RoutingNode(17001, 78), new RoutingNode(10001, 7),new RoutingNode(21009,1),  },17009, 10009,21001)/*,
                new AdjacencyTable(new[] { new RoutingNode(20001, 1)},20009),
                new AdjacencyTable(new[] { new RoutingNode(21001, 1)},21009)*/
            };

            var links = new[]
            {
                new int[] { 15001, 18001,20001 },
                new int[] { 15009, 16001, 11001 },
                new int[] { 16009, 12009, 17001 },
                new int[] { 18009, 12001, 19001 },
                new int[] { 11009, 19009, 10001 },
                new int[] { 17009, 10009,21001 },
                new int[] { 20009,21009 },
            };
            ConfigureCost();


            CostConfiguration.AllPorts = links.SelectMany(n => n).ToList();//Donne tous les ports de tous le monde

            if (algoType == 1)
            {
                InitializeRouters(tables, links, (t, l) => new LSSender(t, l, links.SelectMany(x=>x)));
            }
            else if (algoType == 2)
            {
                InitializeRouters(tables, links, (t, l) => new DV(t, l, links.SelectMany(n => n)));
            }
            Host host1 = new Host(20009, 20001);
            Host host2 = new Host(21009, 21001);
            Thread.Sleep(5000);
            host1.SendMessage("Hello World", 21009);
            Console.Read();
        }


        private static void ConfigureCost()
        {
            //Routeur A
            CostConfiguration.AddCost(15001, 15009, 5);
            CostConfiguration.AddCost(18009, 18001, 45);
            CostConfiguration.AddCost(15001, 18001, 0);
            //HostA
            CostConfiguration.AddCost(20001, 15001, 0);
            CostConfiguration.AddCost(20001, 18001, 0);
            CostConfiguration.AddCost(20001, 20009, 1);
            //Routeur B
            CostConfiguration.AddCost(16001, 16009, 70);
            CostConfiguration.AddCost(11001, 11009, 3);
            CostConfiguration.AddCost(15001, 15009, 5);
            CostConfiguration.AddCost(16001, 15009, 0);
            CostConfiguration.AddCost(15009, 11001, 0);
            CostConfiguration.AddCost(16001, 11001, 0);
            //Routeur C
            CostConfiguration.AddCost(16009, 16001, 70);
            CostConfiguration.AddCost(12009, 12001, 50);
            CostConfiguration.AddCost(17001, 17009, 78);
            CostConfiguration.AddCost(16009, 12009, 0);
            CostConfiguration.AddCost(12009, 17001, 0);
            CostConfiguration.AddCost(16009, 17001, 0);
            //Routeur D
            CostConfiguration.AddCost(18009, 18001, 45);
            CostConfiguration.AddCost(12001, 12009, 50);
            CostConfiguration.AddCost(19001, 19009, 8);
            CostConfiguration.AddCost(18009, 12001, 0);
            CostConfiguration.AddCost(12001, 19001, 0);
            CostConfiguration.AddCost(19001, 18009, 0);
            //Routeur E
            CostConfiguration.AddCost(19009, 19001, 8);
            CostConfiguration.AddCost(11009, 11001, 3);
            CostConfiguration.AddCost(10001, 10009, 7);
            CostConfiguration.AddCost(19009, 11009, 0);
            CostConfiguration.AddCost(11009, 10001, 0);
            CostConfiguration.AddCost(10001, 19009, 0);
            //Routeur F
            CostConfiguration.AddCost(17009, 17001, 78);
            CostConfiguration.AddCost(10009, 10001, 7);
            CostConfiguration.AddCost(17009, 10009, 0);
            //HostB
            CostConfiguration.AddCost(21009,21001,1);
            CostConfiguration.AddCost(21001, 17009, 0);
            CostConfiguration.AddCost(21001, 10009, 0);
        }

        static void InitializeRouters(IEnumerable<AdjacencyTable> tables, IEnumerable<int[]> links, Func<AdjacencyTable, int[], RoutingSender> creator)
        {
            Console.WriteLine("Creating Tables");
            var routers = tables.Zip(links, (table, link) => new Router(creator(table, link))).ToArray();
            foreach (var router in routers)
            {
                router.StartListening();
            }
            List<Task> creatingTables = new List<Task>();
            foreach (var router in routers)
            {
               creatingTables.Add(router.Start());
            }
            Task.WaitAll(creatingTables.ToArray());
            Thread.Sleep(500);
            Console.Out.Flush();
            Console.WriteLine("Tables done!");

        }
    }
}
