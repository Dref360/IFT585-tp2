using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{

    class LS : IRoutingAlgorithm
    {
        
        private List<int> nodes;
        private Dictionary<int, int> cost; 
        public void UpdateRoute()
        {
            
        }

        public int GetRoute(int port)
        {
            throw new NotImplementedException();
        }

        public void UpdateRoute(byte[] readBuffer, int packetSize)
        {
            throw new NotImplementedException();
        }
    }
}
