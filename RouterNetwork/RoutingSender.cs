﻿using System;
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
        private int p1;
        private int p2;
        private int p3;
        public List<int> Ports { get; set; }

        


        public RoutingSender(AdjacencyTable table, int[] ports)
        {
            Table = table;
            Ports = ports.ToList();
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
