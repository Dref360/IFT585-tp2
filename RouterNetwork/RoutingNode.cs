using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    [DataContract]
    class RoutingNode : IComparable<RoutingNode>
    {
        [DataMember]
        public Guid RouterId { get; set; }
        [DataMember]
        public int Cost { get; set; }
        public override int GetHashCode()
        {
            return RouterId.GetHashCode();
        }

        public int CompareTo(RoutingNode other)
        {
            return Cost.CompareTo(other.Cost);
        }
    }
}
