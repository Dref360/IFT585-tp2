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
            //var guidA = Guid.NewGuid();
            //var guidB = Guid.NewGuid();
            //var guidC = Guid.NewGuid();
            //var guidD = Guid.NewGuid();
            //var guidE = Guid.NewGuid();
            //var guidF = Guid.NewGuid();
            var tables = new[] 
            {
                new AdjacencyTable(new[] { new RoutingNode(15009, 5), new RoutingNode(18009, 45) }),
                new AdjacencyTable(new[] { new RoutingNode(15001, 5), new RoutingNode(16009, 70), new RoutingNode(11009, 3) }),
                new AdjacencyTable(new[] { new RoutingNode(16001, 70), new RoutingNode(12001, 50), new RoutingNode(17009, 78) }),
                new AdjacencyTable(new[] { new RoutingNode(18001, 45), new RoutingNode(12009, 50), new RoutingNode(19009, 8) }),
                new AdjacencyTable(new[] { new RoutingNode(11001, 3), new RoutingNode(19001, 8), new RoutingNode(10009, 7) }),
                new AdjacencyTable(new[] { new RoutingNode(17001, 78), new RoutingNode(10001, 7) })
            };
            var links = new[]
            {
                new int[] { 15001, 18001 },
                new int[] { 15009, 16001, 11001 },
                new int[] { 16009, 12009, 17001 },
                new int[] { 18009, 12001, 19001 },
                new int[] { 11009, 19009, 10001 },
                new int[] { 17009, 10009 }
            };

            if (algoType == 1)
            {
                InitializeRouters(tables, links, (t, l) =>
                    new LSSender(t, l,
                        tables.Select(x => 
                            new AdjacencyTable(x.Nodes.Select(y => new RoutingNode(y.Port, int.MaxValue))))));
            }
            else if (algoType == 2)
            {
                InitializeRouters(tables, links, (t, l) => new DV(t, l, links.SelectMany(n => n)));
            }
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
