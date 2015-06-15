using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class DV : IRoutingAlgorithm
    {
        private class CostTable
        {
            private Guid id;
            private int[,] costs;
            private Dictionary<Guid /*Destination*/, int /*row*/> rowDestination;
            private Dictionary<Guid /*Adjacent*/, int /*column*/> columnAdjacent;

            /// <summary>
            /// Construction de la table et initialisation des couts de base
            /// </summary>
            /// <param name="routers">La liste de tous les routeurs</param>
            /// <param name="adjacencyTable">La table des routeurs adjacents</param>
            public CostTable(IList<RoutingNode> routers, AdjacencyTable adjacencyTable)
            {
                id = adjacencyTable.Id;
                IList<RoutingNode> adjacents = adjacencyTable.Nodes as IList<RoutingNode>;
                rowDestination = new Dictionary<Guid, int>();
                columnAdjacent = new Dictionary<Guid, int>();
                costs = new int[routers.Count, adjacents.Count];

                //initailisation de la table de couts et des adjacents/destination
                for (int i = 0; i < routers.Count; i++)
                {
                    rowDestination.Add(routers[i].RouterId, i);
                    for (int j = 0; j < adjacents.Count;j++)
                        costs[i, j] = int.MaxValue;
                }
                for (int i = 0; i < adjacents.Count; i++)
                {
                    columnAdjacent.Add(adjacents[i].RouterId, i);
                    this[adjacents[i].RouterId, adjacents[i].RouterId] = adjacents[i].Cost;
                }
            }

            /// <summary>
            /// On met à jour la table avec les valeurs de la table voisine
            /// </summary>
            /// <param name="otherTable">Table adjacente</param>
            /// <returns>Vrai si on doit envoyer sa table à tous les adjacents</returns>
            public bool UpdateTable(CostTable otherTable)
            {
                bool send = false;
                foreach (Guid destination in rowDestination.Keys)
                {
                    int otherCost = otherTable.BestPath(destination).Cost + this[destination, otherTable.id];
                    if (otherCost < BestPath(destination).Cost)
                    {
                        send = true;
                    }
                    this[destination, otherTable.id] = otherCost;
                }
                return send;
            }

            /// <summary>
            /// Va chercher le meilleur chemin pour aller à une destination
            /// </summary>
            /// <param name="destination">La destination</param>
            /// <returns>Le Node adjacent à prendre</returns>
            public RoutingNode BestPath(Guid destination)
            {
                RoutingNode path = new RoutingNode();
                path.Cost = int.MaxValue;

                foreach (Guid adjacent in columnAdjacent.Keys)
                {
                    if (this[destination, adjacent] < path.Cost)
                    {
                        path.Cost = this[destination, adjacent];
                        path.RouterId = adjacent;
                    }
                }
                return path;
            }

            /// <summary>
            /// Permet d'utiliser la table des couts avec les id des routeurs
            /// </summary>
            /// <param name="destination">routeur destination</param>
            /// <param name="adjacent">routeur adjacent</param>
            /// <returns></returns>
            public int this[Guid destination, Guid adjacent]
            {
                get
                {
                    return costs[rowDestination[destination], columnAdjacent[adjacent]];
                }
                set
                {
                    costs[rowDestination[destination], columnAdjacent[adjacent]] = value;
                }
            }
        }

        CostTable costTable;

        public void CreateRoutingTable(AdjacencyTable table)
        {
            //FAUT POGNER LA LISTE DES ROUTEURS
            costTable = new CostTable(null/*hihi*/, table);
            //SEND
        }

        public EventHandler<MessageArgs> SendMessage { get; set; }
        public void HandleRequests(MessageArgs message)
        {
            //RECEIVE
            CostTable otherCosts = null; //hihi
            if (costTable.UpdateTable(otherCosts))
            {
                //SEND
            }
        }

        public Guid GetRoute(Guid id)
        {
            return costTable.BestPath(id).RouterId;
        }

        private void SerialiseTable()
        {

        }
    }
}
