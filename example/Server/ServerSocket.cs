using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OribiosNetwork.network
{
    interface ServerSocket
    {
        void listen(int port);

        DataSocket handle();
        void Close();
    }
}
