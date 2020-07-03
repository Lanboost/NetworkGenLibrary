using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNetwork
{
    public interface IClientSocket
    {
        void connect(string server, int port);
        bool isError();
        IDataSocket handle();
    }
}
