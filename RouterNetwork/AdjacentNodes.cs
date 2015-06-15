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
        public Guid Id { get; private set; }
        [DataMember]
        public IEnumerable<RoutingNode> Nodes { get; private set; }
        public AdjacencyTable(Guid id, IEnumerable<RoutingNode> nodes)
        {
            Id = id;
            Nodes = nodes;
        }
    }
}
