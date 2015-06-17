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
        public int Port { get; set; }
        [DataMember]
        public int Cost { get; set; }
        public override int GetHashCode()
        {
            return Port.GetHashCode();
        }

        public override string ToString()
        {
            return "{ Guid : " + "" + ", Cost : " + ((Cost==int.MaxValue)?"inf.":Cost.ToString()) + " }";
        }

        public int CompareTo(RoutingNode other)
        {
            return Cost.CompareTo(other.Cost);
        }

        public RoutingNode(int routerId, int cost)
        {
            Port = routerId;
            Cost = cost;
        }

        public RoutingNode()
        {

        }
    }
}
