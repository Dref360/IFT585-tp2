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
        public int Id { get; set; }
        [DataMember]
        public int Cost { get; set; }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return "{ Idd : " + Id + ", Cost : " + ((Cost==int.MaxValue)?"inf.":Cost.ToString()) + " }";
        }

        public int CompareTo(RoutingNode other)
        {
            return Cost.CompareTo(other.Cost);
        }

        public bool IsAdjacent(RoutingNode other)
        {
            return (Id / 10) == (other.Id / 10);
        }

        public RoutingNode(int port, int cost)
        {
            Id = port;
            Cost = cost;
        }

        public RoutingNode()
        {

        }
    }
}
