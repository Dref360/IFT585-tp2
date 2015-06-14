using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class DV : IRoutingAlgorithm
    {
        public void CreateRoutingTable(AdjacencyTable table)
        {
            throw new NotImplementedException();
        }

        public EventHandler<MessageArgs> SendMessage { get; set; }
        public void HandleRequests(MessageArgs message)
        {
            throw new NotImplementedException();
        }


        public Guid GetRoute(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
