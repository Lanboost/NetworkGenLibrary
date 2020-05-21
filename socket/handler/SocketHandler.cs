using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using OribiosNetwork.network;
using OribiosNetwork.network.normal;
using NSServer;

using Connection = OribiosNetwork.network.DataSocket<NSServer.Player>;

namespace OribiosWarServer.engine.socket
{

	public abstract class ServerSocketListener
	{
		public abstract void OnConnected(Connection connection);
		public abstract void OnDisconnected(Connection connection);
		public abstract void OnError(Connection connection);
	}

	public class SocketHandler
	{
		ServerSocket serversocket = new NetSocketServerSocket();

		List<Connection> conns = new List<Connection> ();

		public List<PacketVisitor> visitors = new List<PacketVisitor>();

		public ServerSocketListener Listener { get; set; }

		public int getConnectionCount()
		{
			return conns.Count;
		}

		public SocketHandler(int port = 8888)
		{
			serversocket.listen(8888);
		}

		public void Close()
		{
			serversocket.Close();
		}

		private int lastping = 0;

		public int PingRate { get; set; }


		public void handle(int tick)
        {
			Connection temp = null;

            temp = serversocket.handle();
            if (temp != null)
            {                
                conns.Add(temp);
				Listener.OnConnected(temp);
			}


			if (lastping + PingRate <= lastping)
            {
				lastping = lastping + PingRate;
                List<Connection> bad = new List<Connection>();
                foreach (Connection socket in conns)
                {
                    if (socket.isError())
                    {
                        bad.Add(socket);
                    }
                    else
                    {
                        //PacketStream.Write(socket, new ServerKeepAlivePing(socket.Ping, pingindex));
                        if (socket.isError())
                        {
                            bad.Add(socket);
                        }
                    }
                    
                }
                foreach (Connection socket in bad)
                {
					Listener.OnDisconnected(socket);

					socket.close();
                    conns.Remove(socket);
                }
            }


            foreach (Connection connection in conns)
            {

                connection.handle();

                byte[] message = connection.getMessage();
                while (message != null)
                {
                    if (message.Length < 4)
                    {
                        connection.setError();
                        continue;
                    }
                    BinaryReader reader = new BinaryReader(new MemoryStream(message));

                    Packet pp = PacketStream.read(reader);


                    /*if (pp is ClientKeepAlivePing)
                    {
                        
                    }
                    else
                    {*/
					
                        foreach (PacketVisitor p in visitors)
                        {
                            if (pp.used)
                            {
                                break;
                            }
                            pp.visit(connection, p);
                        }

                        if (!pp.used)
                        {
                            Debug.Log("Packet is not used!");
                            Debug.Log(pp.ToString());
							connection.setError();
                        }
                    //}
                    message = connection.getMessage();
                }
            }

        }
    }

}
