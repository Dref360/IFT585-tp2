using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class AdjacencyTable
    {
        public Guid Id { get; private set; }
        public IEnumerable<AdjacencyTable> Nodes { get; private set; }
        public AdjacencyTable(Guid id, IEnumerable<AdjacencyTable> nodes)
        {
            Id = id;
            Nodes = nodes;
        }
    }
}
