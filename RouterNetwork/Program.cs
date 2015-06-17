using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bonjour, quel algorithme voulez-vous utiliser?\n\t 1:LS\n\t 2:DV");
            int algoType = int.Parse(Console.ReadLine());
            var guidA = Guid.NewGuid();
            var guidB = Guid.NewGuid();
            var guidC = Guid.NewGuid();
            var guidD = Guid.NewGuid();
            var guidE = Guid.NewGuid();
            var guidF = Guid.NewGuid();
            var tables = new[] 
            {
                new AdjacencyTable(guidA, new[] { new RoutingNode(guidB, 5), new RoutingNode(guidD, 45) }),
                new AdjacencyTable(guidB, new[] { new RoutingNode(guidA, 5), new RoutingNode(guidC, 70), new RoutingNode(guidE, 3) }),
                new AdjacencyTable(guidC, new[] { new RoutingNode(guidB, 70), new RoutingNode(guidD, 50), new RoutingNode(guidF, 78) }),
                new AdjacencyTable(guidD, new[] { new RoutingNode(guidA, 45), new RoutingNode(guidC, 50), new RoutingNode(guidE, 8) }),
                new AdjacencyTable(guidE, new[] { new RoutingNode(guidB, 3), new RoutingNode(guidD, 8), new RoutingNode(guidF, 7) }),
                new AdjacencyTable(guidF, new[] { new RoutingNode(guidC, 78), new RoutingNode(guidE, 7) })
            };
            var links = new[]
            {
                new int[] { 15001, 18001 },
                new int[] { 15009, 16001, 11001 },
                new int[] { 6009, 12009, 17001 },
                new int[] { 18009, 12001, 19001 },
                new int[] { 11009, 19009, 10001 },
                new int[] { 17009, 10009 }
            };

            if (algoType == 1)
            {
                InitializeRouters(tables.Take(1), links.Take(1), (t, l) =>
                    new LSSender(t, l,
                        tables.Select(x => 
                            new AdjacencyTable(x.Id, x.Nodes.Select(y => new RoutingNode(y.RouterId, int.MaxValue))))));
            }
            else if (algoType == 2)
            {
                InitializeRouters(tables, links, (t, l) => new DV(t, l, tables.Select(x => x.Id)));
            }
            Console.Read();
        }

        static void InitializeRouters(IEnumerable<AdjacencyTable> tables, IEnumerable<int[]> links, Func<AdjacencyTable, int[], RoutingSender> creator)
        {
            var routers = tables.Zip(links, (table, link) => new Router(creator(table, link)));
            foreach (var router in routers)
            {
                router.StartListening();
            }
            foreach (var router in routers)
            {
                router.Start();
            }

        }
    }
}
