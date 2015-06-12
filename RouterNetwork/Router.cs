using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    enum Algorithm
    {
        LS,DV
    }
    class Router
    {
        private List<List<int>> neighborhood;
        private Dictionary<int, TcpClient> clients;
        private List<int> routerPorts; 
        public int Port { get; set; }
        public IAlgorythm Algorithm { get; set; }
        private Guid guid;


        public Router(int port,IAlgorythm algorythm)
        {
            Port = port;
            Algorithm = algorythm;
            neighborhood = new List<List<int>>();
            routerPorts = new List<int>();
            clients = new Dictionary<int, TcpClient>();
            guid = new Guid();
        }

        public void Listen(TcpClient client,int port)
        {
            while (true)
            {
                byte[] readBuffer = new byte[client.ReceiveBufferSize];
                int packetSize = client.GetStream().Read(readBuffer, 0, readBuffer.Length);
                if (readBuffer[0] == 1)
                {
                    UpdateMap(readBuffer, packetSize);
                }
                else
                {
                    int destPort = BitConverter.ToInt32(readBuffer, 1);
                    if (routerPorts.Any(n => n == destPort))
                    {
                        //ON a un msg TODO
                    }
                    else
                    {
                        int next = Algorithm.GetRoute(destPort);
                        SendData(next, readBuffer,packetSize);
                    }
                }
            }
        }

        private void UpdateMap(byte[] readBuffer, int packetSize)
        {
            Algorithm.UpdateRoute(readBuffer,packetSize);
        }

        private void SendData(int next, byte[] readBuffer,int size)
        {
            if (clients.ContainsKey(next))
            {
                clients[next].GetStream().Write(readBuffer,0,size);
            }
            else
            {
                Console.WriteLine("ERROR:On a pas ce routeur {0}",next);
            }
        }

        public void Link(int portThis,int portDestination,int cost)
        {
            routerPorts.Add(portThis);
            neighborhood[portThis][portDestination] = cost;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, portThis);
            TcpClient tcpClient = new TcpClient(endPoint);
            clients[portDestination] = tcpClient;
            tcpClient.Connect(IPAddress.Loopback, portDestination);
            Task.Factory.StartNew(() => Listen(tcpClient,portDestination));
            SendTable();
        }

        private void SendTable()
        {
            byte[] map;
            foreach (var voisin in neighborhood)
            {
            }
        }
    }
}
