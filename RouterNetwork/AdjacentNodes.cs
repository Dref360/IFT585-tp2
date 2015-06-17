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
        public IEnumerable<RoutingNode> Nodes { get; private set; }
        public AdjacencyTable(IEnumerable<RoutingNode> nodes)
        {
            Nodes = nodes;
        }
    }
}
