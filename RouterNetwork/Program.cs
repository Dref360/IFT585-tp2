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
            var a = new AdjacencyTable(guidA, new[] { new RoutingNode(guidB, 5), new RoutingNode(guidD, 45) });
            var b = new AdjacencyTable(guidB, new[] { new RoutingNode(guidA, 5), new RoutingNode(guidC, 70), new RoutingNode(guidE, 3) });
            var c = new AdjacencyTable(guidC, new[] { new RoutingNode(guidB, 70), new RoutingNode(guidD, 50), new RoutingNode(guidF, 78) });
            var d = new AdjacencyTable(guidD, new[] { new RoutingNode(guidA, 45), new RoutingNode(guidC, 50), new RoutingNode(guidE, 8) });
            var e = new AdjacencyTable(guidE, new[] { new RoutingNode(guidB, 3), new RoutingNode(guidD, 8), new RoutingNode(guidF, 7) });
            var f = new AdjacencyTable(guidF, new[] { new RoutingNode(guidC, 78), new RoutingNode(guidE, 7) }); 


            if(algoType == 1)
            {

            }
        }
    }
}
