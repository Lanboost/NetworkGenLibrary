using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNetwork
{
    public interface ServerSocket
    {
        void listen(int port);

        DataSocket handle();
        void Close();
    }
}
