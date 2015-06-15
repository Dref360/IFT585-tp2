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
    enum Algorithm
    {
        LS,DV
    }

    enum Header
    {
        NEW = 0,
        UPDATE =1,
        DATA = 2
    }
    class Router
    {
        private Dictionary<Guid, int> routersPort;
        private List<TcpListener> listeners; 

        public List<int> Ports { get; set; }
        public IRoutingAlgorithm Algorithm { get; set; }
        private Guid guid;


        public Router(IRoutingAlgorithm algorythm, int[] ports)
        {
            Ports = ports.ToList();
            Algorithm = algorythm;
            routersPort = new Dictionary<Guid, int>();
            guid = new Guid();
            listeners = new List<TcpListener>();
            Algorithm.SendMessage += SendMessage;
            StartListening();
        }

        private void SendMessage(object sender, MessageArgs e)
        {
            if (!routersPort.ContainsKey(e.Receiver))
            {
                Console.WriteLine("On a pas ce routeur (Bruno que fais-tu?)");
                return;
            }
            e.Sender = this.guid;
            byte[] msg = SerializeMsg(e);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any,0);
            TcpClient client = new TcpClient(endPoint);
            client.Connect(IPAddress.Loopback,routersPort[e.Receiver]);
            client.GetStream().Write(msg,0,msg.Length);
            if (e.ExpectResponse)
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
                Algorithm.HandleRequests(DeserializeMessageArgs(allBytes.ToArray()));
            }
            client.Close();
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
                formatter.Serialize(ms,messageArgs);
                return ms.ToArray();
            }
        }

        public void InitializeLS(List<RoutingNode> routers)
        {
            
        }

        public void InitializeDV(List<RoutingNode> routers)
        {

        }

        private void StartListening()
        {
            foreach (var port in Ports)
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback,port);
                listeners.Add(listener);
                Task.Factory.StartNew(() => AcceptConnection(listener));
            }
        }

        private void AcceptConnection(TcpListener listener)
        {
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
                sock.Close();
            }
        }


        public void Link(Guid router,int port)
        {
            routersPort[router] = port;
        }

    }
}
