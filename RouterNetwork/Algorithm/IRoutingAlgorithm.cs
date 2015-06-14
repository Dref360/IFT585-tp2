using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    //DS or LS
    interface IRoutingAlgorithm
    {
        /// <summary>
        /// Crée la table de routage pour une représentation interne de la table
        /// </summary>
        /// <param name="table"></param>
        void CreateRoutingTable(AdjacencyTable table);
        /// <summary>
        /// Permet a l'algorithme de routage d'envoyer des demandes aux autres routeurs
        /// </summary>
        EventHandler<MessageArgs> SendMessage { get; set; }
        /// <summary>
        /// Traite les requêtes de table de routage
        /// </summary>
        /// <param name="message"></param>
        void HandleRequests (MessageArgs message);
        /// <summary>
        /// Retourne la route optimal à prendre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Guid GetRoute(Guid id);
    }
}
