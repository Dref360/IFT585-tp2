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
    enum Algorithm
    {
        LS,DV
    }

    enum Header
    {
        NEW = 0,
        UPDATETABLE =1,
        DATA = 2
    }
    class Router
    {
        private List<TcpListener> listeners; 

        private RoutingSender sender;

        public Router(RoutingSender sender)
        {
            this.sender = sender;
            listeners = new List<TcpListener>();
        }

        public Task Start()
        {
            return Task.Factory.StartNew(sender.CreateRoutingTable);
        }


        public void StartListening()
        {
            foreach (var port in sender.Ports)
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback,port);
                listeners.Add(listener);
                Task.Factory.StartNew(() => AcceptConnection(listener));
            }
        }

        private void AcceptConnection(TcpListener listener)
        {
            listener.Start();
            while (true)
            {
                byte[] bytes = new byte[64000];
                TcpClient sock = listener.AcceptTcpClient();
                var stream = sock.GetStream();
                List<byte> allBytes = new List<byte>(64000);
                int i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    allBytes.AddRange(bytes.Take(i));
                    if (!stream.DataAvailable)
                        break;
                }
                var msg = MessageArgs.DeserializeMessageArgs(allBytes.ToArray());
                var response = sender.HandleRoutingRequests(msg);
                if (msg.ExpectResponse)
                {
                    if(response == null)
                        Console.WriteLine("L'autre routeur veut une reponse, mais la réponse est vide");
                    byte[] data = response.SerializeMsg();
                    stream.Write(data, 0, data.Length);
                }
                sock.Close();
            }
        }


        

    }
}
