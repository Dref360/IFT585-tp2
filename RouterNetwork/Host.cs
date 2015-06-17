using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class Host 
    {
        int Port { get; set; }
        int RouterPort { get; set; }
        public Host(int port, int routerPort)
        {
            Port = port;
            RouterPort = routerPort;
            Task.Factory.StartNew(AcceptConnection);
        }

        private void AcceptConnection()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback,Port);
            while (true)
            {
                byte[] bytes = new byte[64000];
                TcpClient sock = listener.AcceptTcpClient();
                var stream = sock.GetStream();
                List<byte> allBytes = new List<byte>(64000);
                int i;
                bool received = false;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0 || !received)
                {
                    if (i == 0)
                        continue;
                    received = true;
                    allBytes.AddRange(bytes.Take(i));
                }
                Console.WriteLine("Message recu!");
                Console.WriteLine(BitConverter.ToString(MessageArgs.DeserializeMessageArgs(allBytes.ToArray()).Data));
                sock.Close();
            }
        }

        public void SendMessage(string msg, int host2)
        {
            
            MessageArgs argsMsg = new MessageArgs()
            {
                Sender = Port,
                Data = System.Text.Encoding.Default.GetBytes(msg),
                ExpectResponse = false,
                NextPoint = RouterPort,
                Header = new byte[] { 1},Receiver = host2
            };
            TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Loopback,0));
            client.Connect(IPAddress.Loopback,RouterPort);
            var data = argsMsg.SerializeMsg();
            client.GetStream().Write(data,0,data.Length);
            client.Close();
        }


    }
}
