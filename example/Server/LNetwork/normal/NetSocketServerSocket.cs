using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LNetwork.normal
{
    public class NetSocketServerSocket : IServerSocket
    {
        Socket socket;
        SocketAsyncEventArgs asyncevent = new SocketAsyncEventArgs();
        int state = 0;
        IDataSocket dataSocket;


        public IDataSocket handle()
        {
            //do async
            if (state == 0)
            {
                state = 1;
                if (!socket.AcceptAsync(asyncevent))
                {
                    acceptConnection(null, null);
                }
            }
            //wait for async
            else if (state == 1)
            {

            }
            //return a waiting socket
            else if (state == 2)
            {
                state = 0;
                asyncevent.AcceptSocket = null;
                return dataSocket;
            }
            return null;
        }

        public void acceptConnection(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket s = asyncevent.AcceptSocket;
                if (s != null)
                {
                    dataSocket = new NetSocketDataSocket(s);
                    state = 2;
                }
                else
                {
                    state = 0;
                }
            }
            else
            {
            }
        }

        public void listen(int port)
        {
            socket = new Socket(AddressFamily.InterNetwork,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ep);

            socket.Listen(10);

            asyncevent.Completed += acceptConnection;
        }

        public void Close()
        {
            socket.Close();
        }
    }
}
