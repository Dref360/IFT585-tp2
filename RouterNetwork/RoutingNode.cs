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

        public override string ToString()
        {
            return "{ Guid : " + RouterId + ", Cost : " + ((Cost==int.MaxValue)?"inf.":Cost.ToString()) + " }";
        }

        public int CompareTo(RoutingNode other)
        {
            return Cost.CompareTo(other.Cost);
        }

        public RoutingNode(Guid routerId, int cost)
        {
            RouterId = routerId;
            Cost = cost;
        }

        public RoutingNode()
        {

        }
    }
}
