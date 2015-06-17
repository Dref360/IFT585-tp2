using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    static class CostConfiguration
    {
        public static Dictionary<Tuple<int, int>, int> NetworkCosts = new Dictionary<Tuple<int, int>, int>(); 
        public static List<int> AllPorts = new List<int>(); 

        public static void AddCost(int port, int port2, int cost)
        {
            NetworkCosts[Tuple.Create(port, port2)] = cost;
        }

        public static int Cost(int port1, int port2)
        {
            if (NetworkCosts.ContainsKey(Tuple.Create(port1, port2)))
                return NetworkCosts[Tuple.Create(port1, port2)];
            if (NetworkCosts.ContainsKey(Tuple.Create(port2, port1)))
                return NetworkCosts[Tuple.Create(port2, port1)];
            return 1000000;
        }

        public static IEnumerable<int> SameRouter(int port)
        {
            return AllPorts.Where(x => Cost(port, x) == 0);
        }
    }
}
