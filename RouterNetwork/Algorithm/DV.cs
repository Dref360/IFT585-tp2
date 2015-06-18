using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace RouterNetwork
{
    class DV : RoutingSender
    {
        private const int infini = 1000000;

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
            public CostTable(IList<int> routers, AdjacencyTable adjacencyTable)
            {
                IList<RoutingNode> adjacents = adjacencyTable.Nodes as IList<RoutingNode>;
                rowDestination = new Dictionary<int, int>();
                columnAdjacent = new Dictionary<int, int>();
                costs = new int[routers.Count, adjacents.Count];

                //initailisation de la table de couts et des adjacents/destination
                for (int i = 0; i < routers.Count; i++)
                {
                    rowDestination.Add(routers[i], i);
                    for (int j = 0; j < adjacents.Count; j++)
                        costs[i, j] = infini;
                }
                for (int i = 0; i < adjacents.Count; i++)
                {
                    columnAdjacent.Add(adjacents[i].Id, i);
                    this[adjacents[i].Id, adjacents[i].Id] = adjacents[i].Cost;
                }
            }

            /// <summary>
            /// Constructeur de copie
            /// </summary>
            /// <param name="table"></param>
            public CostTable(CostTable table)
            {
                this.costs = new int[table.rowDestination.Count, table.columnAdjacent.Count];
                Array.Copy(table.costs, this.costs, table.costs.Length);
                this.rowDestination = table.rowDestination;
                this.columnAdjacent = table.columnAdjacent;
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
                    RoutingNode otherBestPath = otherTable.BestPath(destination);
                    if (destination != sender && otherBestPath.Id != 0)
                    {
                        int otherCost = otherBestPath.Cost + this[sender, sender];
                        if (otherCost < BestPath(destination).Cost)
                        {
                            send = true;
                        }
                        this[destination, sender] = Math.Min(infini, otherCost);
                    }
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
                path.Cost = infini;
                path.Id = 0;

                foreach (int adjacent in columnAdjacent.Keys)
                {
                    if (this[destination, adjacent] < path.Cost)
                    {
                        path.Cost = this[destination, adjacent];
                        path.Id = adjacent;
                    }
                }
                return path;
            }

            /// <summary>
            /// Créé une nouvelle table empoisonné à partir de la table courante
            /// </summary>
            /// <param name="adjacent"></param>
            /// <returns></returns>
            public CostTable CreatePoisonedTable(RoutingNode adjacent)
            {
                CostTable poisonedTable = new CostTable(this);
                foreach (int destination in rowDestination.Keys)
                    poisonedTable[destination, adjacent.Id] = infini;
                return poisonedTable;
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

        IEnumerable<int> allPorts;
        CostTable costTable;


        public DV(AdjacencyTable table, int[] ports, IEnumerable<int> ids)
            : base(table, ports)
        {
            this.allPorts = ids;
        }

        public override void CreateRoutingTable()
        {
            costTable = new CostTable(allPorts.ToList(), Table);
            Console.WriteLine("Table de routage initiale créée");
            SendTable();
        }

        public override MessageArgs HandleRoutingRequests(MessageArgs message)
        {
            while (costTable == null)
                Thread.Sleep(1);
            if (message.Header == 0)
            {
                //Console.WriteLine("Laisse moi updater pls");
                CostTable otherCosts = ReceiveMessage(message);
                if (costTable.UpdateTable(message.Sender, otherCosts))
                {
                    SendTable();
                    //Console.WriteLine("Table mis à jour, envoie de la table");
                }
                //Console.WriteLine("Fin de l'update");
            }
            else if (message.Header == 1)
            {
                int nextStep = costTable.BestPath(message.Receiver).Id;
                Console.WriteLine("Next Step: {0}", nextStep);
                SendMessage(new MessageArgs()
                {
                    Data = message.Data,
                    ExpectResponse = false,
                    Header = message.Header,
                    NextPoint = nextStep,
                    Receiver = message.Receiver
                });
            }
            return null;
        }

        protected override int GetRoute(int id)
        {
            return costTable.BestPath(id).Id;
        }

        private void SendTable()
        {
            foreach (RoutingNode adjacent in Table.Nodes)
            {
                if (adjacent.Id == 20009 || adjacent.Id == 21009)
                    break;
                CostTable poisonedTable = costTable.CreatePoisonedTable(adjacent);

                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, poisonedTable);
                    SendMessage(new MessageArgs()
                    {
                        Data = ms.ToArray(),
                        Receiver = adjacent.Id,
                        NextPoint = adjacent.Id
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
