using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class RoutingNode : IComparable<RoutingNode>
    {
        public Guid RouterId { get; set; }
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
