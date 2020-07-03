using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LNetwork.websocket
{
    
    public class WebSocketServerSocket : IServerSocket
    {

        public WebSocketServerSocket()
        {
            instance = this;
        }

        private static WebSocketServerSocket instance = null;
        public object olock = new object();
        public List<IDataSocket> newsockets = new List<IDataSocket>();


        public IDataSocket handle()
        {
            IDataSocket nsocket = null;
            lock (olock)
            {
                if(newsockets.Count > 0)
                {
                    nsocket = newsockets[0];
                    newsockets.RemoveAt(0);
                }
            }
            return nsocket;
        }

        public void listen(int port)
        {
            var wssv = new WebSocketServer(port);
            wssv.AddWebSocketService<WebSocketDataSocket>("/websocket");
            wssv.Start();


        }

        public static WebSocketServerSocket getInstance()
        {
            return instance;
        }

		public void Close()
		{
			instance.Close();
		}
	}
}
