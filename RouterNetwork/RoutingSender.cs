using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    abstract class RoutingSender
    {
        private Guid Id;
        private Dictionary<Guid, int> routerPorts;
        public List<int> Ports { get; set; }

        


        public RoutingSender(params int[] ports)
        {
            routerPorts = new Dictionary<Guid, int>();
            Ports = ports.ToList();
        }
        private MessageArgs DeserializeMessageArgs(byte[] allBytes)
        {
            using (MemoryStream ms = new MemoryStream(allBytes))
            {
                BinaryFormatter fmt = new BinaryFormatter();
                return fmt.Deserialize(ms) as MessageArgs;
            }
        }

        private byte[] SerializeMsg(MessageArgs messageArgs)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, messageArgs);
                return ms.ToArray();
            }
        }

        public void SendMessage(MessageArgs message)
        {
            if (!routerPorts.ContainsKey(message.Receiver))
            {
                Console.WriteLine("On a pas ce routeur (Bruno que fais-tu?)");
                return;
            }
            message.Sender = this.Id;
            byte[] msg = SerializeMsg(message);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            TcpClient client = new TcpClient(endPoint);
            client.Connect(IPAddress.Loopback, routerPorts[message.Receiver]);
            client.GetStream().Write(msg, 0, msg.Length);
            if (message.ExpectResponse)
            {
                bool received = false;
                byte[] data = new byte[65000];
                var allBytes = new List<byte>();
                var stream = client.GetStream();
                int i;
                while ((i = stream.Read(data, 0, data.Length)) != 0 || !received)
                {
                    if (i == 0)
                        continue;
                    received = true;
                    allBytes.AddRange(data.Take(i));
                }
                //Deserialize msg
                HandleRoutingRequests(DeserializeMessageArgs(allBytes.ToArray()));
            }
            client.Close();
        }

        public void Link(Guid router, int port)
        {
            routerPorts[router] = port;
        }

        /// <summary>
        /// Retourne la route optimal à prendre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        abstract protected Guid GetRoute(Guid id);
        /// <summary>
        /// Traite les requêtes de table de routage
        /// </summary>
        /// <param name="message"></param>
        abstract public void HandleRoutingRequests(MessageArgs args);
        /// <summary>
        /// Crée la table de routage pour une représentation interne de la table
        /// </summary>
        /// <param name="table"></param>
        abstract public void CreateRoutingTable(AdjacencyTable table);

    }
}
