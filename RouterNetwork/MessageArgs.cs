﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    class MessageArgs
    {
        public Guid Receiver { get; set; }
        public byte[] Header { get; set; }
        public byte[] Data { get; set; }
        public bool ExpectResponse { get; set; }
    }
}
