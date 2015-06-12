using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterNetwork
{
    //DS or LS
    interface IAlgorythm
    {
        void UpdateRoute();

        int GetRoute(int port);

        void UpdateRoute(byte[] readBuffer, int packetSize);
    }
}
