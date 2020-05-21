using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OribiosNetwork.network
{
    public interface ClientSocket
    {
        void connect(string server, int port);
        bool isError();
        DataSocket handle();
    }
}
