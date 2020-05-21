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

namespace OribiosNetwork.network
{

	public abstract class ClientSocketListener
	{
		public abstract void OnConnected();
		public abstract void OnDisconnected();
		public abstract void OnError();
	}

	public class ClientSocketHandler
	{
		ClientSocket clientsocket = new NetSocketClientSocket();

		public List<PacketVisitor> visitors = new List<PacketVisitor>();

		public List<Packet> packetQueue = new List<Packet>();
		public bool isQueued = false;

		public ClientSocketListener Listener { get; set; }

		public Connection connection;

		public void connect(string host, int port) {
			clientsocket.connect(host, port);
		}

		public void Close()
		{
			//TODO
		}

		private int lastping = 0;

		public int PingRate { get; set; }


		public void handle(int tick)
        {
			if (connection == null)
			{
				connection = clientsocket.handle();
				if (connection != null)
				{
					Listener.OnConnected();
				}
			}

			if (connection != null)
			{
				if (lastping + PingRate <= lastping)
				{
					lastping = lastping + PingRate;
					List<Connection> bad = new List<Connection>();
					if (connection.isError())
					{
						//bad.Add(socket);
						Listener.OnDisconnected();

						connection.close();
					}
					else
					{
						//PacketStream.Write(socket, new ServerKeepAlivePing(socket.Ping, pingindex));
						if (connection.isError())
						{
							//bad.Add(socket);
							Listener.OnDisconnected();

							connection.close();
						}
					}
					
				}


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


					if(pp is StartSyncPacket) {
						packetQueue.Clear();
						isQueued = true;
					}
					else if(pp is EndSyncPacket) {
						foreach(var qPacket in packetQueue) {
							handlePacket(qPacket);
						}

						packetQueue.Clear();
						isQueued = false;
					}
					else {
						if(isQueued) {
							packetQueue.Add(pp);
						}
						else {
							handlePacket(pp);
						}
					}

					message = connection.getMessage();
				}
			}

        }

		private void handlePacket(Packet packet) {
			foreach (PacketVisitor p in visitors)
			{
				if (packet.used)
				{
					break;
				}
				packet.visit(connection, p);
			}

			if (!packet.used)
			{
				Debug.Log("Packet is not used!");
				Debug.Log(packet.ToString());
				connection.setError();
			}
		}
    }

}
