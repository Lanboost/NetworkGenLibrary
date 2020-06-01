using LNetwork.normal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.service
{
	public enum ServerNetworkLoginResponse
	{
		OK,
		INCORRECT_USER,
		INCORRECT_PASSWORD
	}

	class ConnectionData
	{
		public DataSocket DataSocket { get; set; }
		public int State { get; set; }
		public long Timeout { get; set; }
	}

	public class ServerNetwork: INetworkSocketHandlerServer
	{

		Dictionary<uint, DataSocket> Sockets = new Dictionary<uint, DataSocket>();
		Dictionary<uint, NetworkSocketStateRouter> SocketRoutes = new Dictionary<uint, NetworkSocketStateRouter>();
		IBuilder<ServerSocket> ServerSocketBuilder;
		ServerSocket ServerSocket;
		
		UIDCounter SocketIdCounter = new UIDCounter();

		Action<uint, DataSocket, NetworkSocketStateRouter> onConnected;

		public ServerNetwork(IBuilder<ServerSocket> serverSocketBuilder, Action<uint, DataSocket, NetworkSocketStateRouter> onConnected)
		{
			ServerSocketBuilder = serverSocketBuilder;
			this.onConnected = onConnected;
		}

		public void Listen(int port)
		{
			if(ServerSocket != null)
			{
				throw new Exception("Socket already running!");
			}

			ServerSocket = ServerSocketBuilder.Build();
			ServerSocket.listen(port);
		}

		public void Handle()
		{

			var newSocket = ServerSocket.handle();
			if(newSocket != null)
			{
				//Wrap socket in a RPC ping handler
				var router = new NetworkSocketStateRouter();
				
				var socketId = SocketIdCounter.Get();
				SocketRoutes.Add(socketId, router);
				Sockets.Add(socketId, newSocket);

				onConnected(socketId, newSocket, router);
			}

			List<int> timed = new List<int>();
			foreach (var pair in Sockets)
			{
				pair.Value.handle();
				while (true)
				{
					var msg = pair.Value.getMessage();

					if (msg != null)
					{
						BinaryReader reader = new BinaryReader(new MemoryStream(msg));
						uint cmd = reader.ReadUInt32();

						SocketRoutes[pair.Key].Handle(this, pair.Key, cmd, reader);


					}
					else
					{
						break;
					}
				}
			}

		}

		public IEnumerable<Tuple<uint, DataSocket>> GetSockets()
		{
			foreach (var elem in Sockets)
			{
				yield return Tuple.Create(elem.Key, elem.Value);
			}
		}

		public int GetSocketCount()
		{
			return Sockets.Count;
		}

		public DataSocket GetSocket(uint socketId)
		{
			return Sockets[socketId];
		}

		public NetworkSocketStateRouter GetSocketRouter(uint socketId)
		{
			return SocketRoutes[socketId];
		}

		public void Send(uint socketId, byte[] msg)
		{
			Sockets[socketId].send(msg);
		}

		public void BroadCast(byte[] msg)
		{
			foreach (var elem in Sockets)
			{
				elem.Value.send(msg);
			}
		}
		
	}
}
