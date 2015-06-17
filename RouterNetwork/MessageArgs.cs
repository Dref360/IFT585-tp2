using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    [Serializable]
    [DataContract]
    class MessageArgs
    {
        [DataMember]
        public int Sender { get; set; }
        [DataMember]
        public int Receiver { get; set; }
        [DataMember]
        public int NextPoint { get; set; }
        [DataMember]
        public int Header { get; set; }
        [DataMember]
        public byte[] Data { get; set; }
        [DataMember]
        public bool ExpectResponse { get; set; }

        public static MessageArgs DeserializeMessageArgs(byte[] allBytes)
        {
            using (MemoryStream ms = new MemoryStream(allBytes))
            {
                BinaryFormatter fmt = new BinaryFormatter();
                return fmt.Deserialize(ms) as MessageArgs;
            }
        }

        public byte[] SerializeMsg()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                return ms.ToArray();
            }
        }
    }
}
