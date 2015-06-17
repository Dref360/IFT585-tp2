using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class DV : RoutingSender
    {
        [Serializable]
        private class CostTable
        {
            private int[,] costs;
            private Dictionary<int /*Destination*/, int /*row*/> rowDestination;
            private Dictionary<int /*Adjacent*/, int /*column*/> columnAdjacent;

            /// <summary>
            /// Construction de la table et initialisation des couts de base
            /// </summary>
            /// <param name="routers">La liste de tous les routeurs</param>
            /// <param name="adjacencyTable">La table des routeurs adjacents</param>
            public CostTable(IList<RoutingNode> routers, AdjacencyTable adjacencyTable)
            {
                IList<RoutingNode> adjacents = adjacencyTable.Nodes as IList<RoutingNode>;
                rowDestination = new Dictionary<int, int>();
                columnAdjacent = new Dictionary<int, int>();
                costs = new int[routers.Count, adjacents.Count];

                //initailisation de la table de couts et des adjacents/destination
                for (int i = 0; i < routers.Count; i++)
                {
                    rowDestination.Add(routers[i].Port, i);
                    for (int j = 0; j < adjacents.Count; j++)
                        costs[i, j] = int.MaxValue;
                }
                for (int i = 0; i < adjacents.Count; i++)
                {
                    columnAdjacent.Add(adjacents[i].Port, i);
                    this[adjacents[i].Port, adjacents[i].Port] = adjacents[i].Cost;
                }
            }

            /// <summary>
            /// On met à jour la table avec les valeurs de la table voisine
            /// </summary>
            /// <param name="otherTable">Table adjacente</param>
            /// <returns>Vrai si on doit envoyer sa table à tous les adjacents</returns>
            public bool UpdateTable(int sender,CostTable otherTable)
            {
                bool send = false;
                foreach (int destination in rowDestination.Keys)
                {
                    int otherCost = otherTable.BestPath(destination).Cost + this[destination, sender];
                    if (otherCost < BestPath(destination).Cost)
                    {
                        send = true;
                    }
                    this[destination, sender] = otherCost;
                }
                return send;
            }

            /// <summary>
            /// Va chercher le meilleur chemin pour aller à une destination
            /// </summary>
            /// <param name="destination">La destination</param>
            /// <returns>Le Node adjacent à prendre</returns>
            public RoutingNode BestPath(int destination)
            {
                RoutingNode path = new RoutingNode();
                path.Cost = int.MaxValue;

                foreach (int adjacent in columnAdjacent.Keys)
                {
                    if (this[destination, adjacent] < path.Cost)
                    {
                        path.Cost = this[destination, adjacent];
                        path.Port = adjacent;
                    }
                }
                return path;
            }

            /// <summary>
            /// Empoisonne les couts de la table pour un routeur adjacent
            /// </summary>
            /// <param name="adjacent"></param>
            public void Poison(RoutingNode adjacent)
            {
                foreach (int destination in rowDestination.Keys)
                    this[destination, adjacent.Port] = int.MaxValue;
            }

            /// <summary>
            /// Permet d'utiliser la table des couts avec les id des routeurs
            /// </summary>
            /// <param name="destination">routeur destination</param>
            /// <param name="adjacent">routeur adjacent</param>
            /// <returns></returns>
            public int this[int destination, int adjacent]
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



        public DV(AdjacencyTable table, int[] ports, IEnumerable<int> ids)
            : base(table, ports)
        {

        }

        CostTable costTable;

        public override void CreateRoutingTable()
        {
            //FAUT POGNER LA LISTE DES ROUTEURS
            costTable = new CostTable(null/*hihi*/, Table);
            SendTable();
        }

        public override MessageArgs HandleRoutingRequests(MessageArgs message)
        {
            CostTable otherCosts = ReceiveMessage(message);
            if (costTable.UpdateTable(message.Sender,otherCosts))
                SendTable();
            return null;
        }

        protected override int GetRoute(int id)
        {
            return costTable.BestPath(id).Port;
        }

        private void SendTable()
        {
            foreach (RoutingNode adjacent in Table.Nodes)
            {
                CostTable poisonedTable = costTable;
                poisonedTable.Poison(adjacent);

                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, poisonedTable);
                    SendMessage(new MessageArgs()
                    {
                        Data = ms.ToArray(),
                        Receiver = adjacent.Port,
                        NextPoint = adjacent.Port
                    });
                }
            }
        }

        private CostTable ReceiveMessage(MessageArgs message)
        {
            using (var memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(message.Data, 0, message.Data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                Object obj = binForm.Deserialize(memStream);
                return (CostTable)obj;
            }
        }
    }
}
