using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNetwork
{
    public interface IServerSocket
    {
        void listen(int port);

        IDataSocket handle();
        void Close();
    }
}
