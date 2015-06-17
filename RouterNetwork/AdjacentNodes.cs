using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RouterNetwork
{
    [DataContract]
    class AdjacencyTable
    {
        [DataMember]
        public RouterId routerId { get; set; }
        [DataMember]
        public IEnumerable<RoutingNode> Nodes { get; private set; }
        public AdjacencyTable(IEnumerable<RoutingNode> nodes,params int[] ports)
        {
            routerId = new RouterId(ports);
            Nodes = nodes;
        }
    }
}
