using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OribiosNetwork.network.normal
{
    class NetSocketClientSocket: ClientSocket
    {
        Socket socket;
        SocketAsyncEventArgs asyncevent = new SocketAsyncEventArgs();

        int state = 0;
        bool error;
        string errormessage;

        public bool isError()
        {
            return error;
        }

        public void connect(string server, int port)
        {
            error = false;
            IPHostEntry hostEntry = null;

            // Get host related information.
            hostEntry = Dns.GetHostEntry(server);

            IPAddress ip4 = null;
            foreach (IPAddress address in hostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip4 = address;
                    break;
                }
            }

            IPEndPoint ipe = new IPEndPoint(ip4, port);
            socket =
                new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);



            asyncevent.RemoteEndPoint = ipe;
            asyncevent.Completed += connectResult;
            if (!socket.ConnectAsync(asyncevent))
            {
                connectResult(null, null);
            }
            


        }

        public void connectResult(object sender, SocketAsyncEventArgs e)
        {
            asyncevent.Completed -= connectResult;
            if (asyncevent.SocketError == SocketError.Success)
            {
                state = 1;
            }
            else
            {
                error = true;
                errormessage = "Couldn't connect to server.";
            }
        }

        public DataSocket handle()
        {
            //waiting for connection
            if (state == 0)
            {

            }

            else if (state == 1)
            {
                state++;
                return new NetSocketDataSocket(socket);
            }
            return null;
        }
    }
}
