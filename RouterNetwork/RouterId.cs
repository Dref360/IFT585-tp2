using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    public class RouterId
    {
        protected bool Equals(RouterId other)
        {
            return Equals(Ports, other.Ports);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RouterId) obj);
        }

        public override int GetHashCode()
        {
            return (Ports != null ? Ports.GetHashCode() : 0);
        }

        public int[] Ports;

        public RouterId(int[] port)
        {
            Ports = port;
        }

        public static bool operator !=(RouterId id1, RouterId id2)
        {
            return !(id1 == id2);
        }

        public static bool operator == (RouterId id1, RouterId id2)
        {
            return id1.Ports == id2.Ports;
        }


        public static bool operator !=(RouterId id1, int id2)
        {
            return id1 == id2;
        }
        public static bool operator ==(RouterId id1, int id2)
        {
            return id1.Ports.Contains(id2);
        }

        public override string ToString()
        {
            string s = "";
            foreach (var port in Ports)
            {
                s += string.Format("{0}-",port);
            }
            return s;
        }
    }
}
