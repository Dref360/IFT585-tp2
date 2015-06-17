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
        private int Id;
        protected AdjacencyTable Table { get; set; }
        public IEnumerable<int> Ports { get; set; }

        public RoutingSender(AdjacencyTable table, int[] ports)
        {
            Table = table;
            Ports = ports;
        }


        public MessageArgs SendMessageWithAnswer(MessageArgs message)
        {
            if (!Ports.Any(n => n/10 == message.NextPoint/10))
            {
                Console.WriteLine("On a pas ce routeur (Bruno que fais-tu?)");
                return null;
            }
            message.Sender = Ports.First(n => n / 10 == message.NextPoint / 10);
            byte[] msg = message.SerializeMsg();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            TcpClient client = new TcpClient(endPoint);
            client.Connect(IPAddress.Loopback, message.NextPoint);
            client.GetStream().Write(msg, 0, msg.Length);
            Console.WriteLine("{0} : Sending a request", message.Sender);
            if (message.ExpectResponse)
            {
                byte[] data = new byte[65000];
                var allBytes = new List<byte>();
                var stream = client.GetStream();
                int i;
                while ((i = stream.Read(data, 0, data.Length)) != 0)
                {
                    if (i == 0)
                        continue;
                    if (!stream.DataAvailable)
                        break;
                    allBytes.AddRange(data.Take(i));
                }
                Console.WriteLine("{0} : Received a response",message.Sender);
                //Deserialize msg
                return MessageArgs.DeserializeMessageArgs(allBytes.ToArray());
            }
            client.Close();
            return null;
        }

        public void SendMessage(MessageArgs message)
        {
            if (!Ports.Any(n => n / 10 == message.NextPoint / 10))
            {
                Console.WriteLine("On a pas ce routeur (Bruno que fais-tu?)");
                return;
            }
            message.Sender = Ports.First(n => n / 10 == message.NextPoint / 10);
            byte[] msg = message.SerializeMsg();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            TcpClient client = new TcpClient(endPoint);
            client.Connect(IPAddress.Loopback, message.NextPoint);
            Console.WriteLine("{0} : Sending a request", message.Sender);
            client.GetStream().Write(msg, 0, msg.Length);
            client.Close();
        }

        /// <summary>
        /// Retourne la route optimal à prendre
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        abstract protected int GetRoute(int port);
        /// <summary>
        /// Traite les requêtes de table de routage
        /// </summary>
        /// <param name="message"></param>
        abstract public MessageArgs HandleRoutingRequests(MessageArgs args);
        /// <summary>
        /// Crée la table de routage pour une représentation interne de la table
        /// </summary>
        /// <param name="table"></param>
        public abstract void CreateRoutingTable();

    }
}
