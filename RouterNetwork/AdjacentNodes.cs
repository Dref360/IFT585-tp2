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
        public int[] Ports { get; set; }
        [DataMember]
        public IEnumerable<RoutingNode> Nodes { get; private set; }
        public AdjacencyTable(IEnumerable<RoutingNode> nodes,params int[] ports)
        {
            Ports = ports;
            Nodes = nodes;
        }
    }
}
