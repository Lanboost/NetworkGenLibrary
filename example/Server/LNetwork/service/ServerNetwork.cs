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

	public class ServerNetwork: INetworkSocketHandler
	{

		private static int STATE_CONNECTED = 0;
		private static int STATE_ERROR = 2;

		Dictionary<uint, DataSocket> Sockets = new Dictionary<uint, DataSocket>();
		Dictionary<uint, NetworkSocketStateRouter> SocketRouteStates = new Dictionary<uint, NetworkSocketStateRouter>();
		Dictionary<uint, NetworkSocketState> SocketStates = new Dictionary<uint, NetworkSocketState>();
		IBuilder<ServerSocket> ServerSocketBuilder;
		ServerSocket ServerSocket;
		

		UIDCounter SocketIdCounter = new UIDCounter();
		IBuilder<NetworkSocketState> connectionStateBuilder;
		NetworkPacketIdGenerator gen;
		public ServerNetwork(NetworkPacketIdGenerator gen, IBuilder<ServerSocket> serverSocketBuilder,IBuilder<NetworkSocketState> connectionStateBuilder)
		{
			ServerSocketBuilder = serverSocketBuilder;

			this.connectionStateBuilder = connectionStateBuilder;
			this.gen = gen;
		}

		int cidCounter = 0;

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
				var pingState = new PingNetworkState(gen);
				router.Attach(pingState);
				var connectionState = connectionStateBuilder.Build();

				router.Attach(connectionState);
				var socketId = SocketIdCounter.Get();
				SocketRouteStates.Add(socketId, router);
				SocketStates.Add(socketId, connectionState);
				Sockets.Add(socketId, newSocket);
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

						SocketStates[pair.Key].Handle(this, pair.Key, cmd, reader);


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

		public NetworkSocketState GetSocketState(uint socketId)
		{
			return SocketStates[socketId];
		}

		public void Send(uint socketId, byte[] msg)
		{
			Sockets[socketId].send(msg);
		}

		public void Send(byte[] msg)
		{
			throw new NotImplementedException();
		}

		public void BroadCast(byte[] msg)
		{
			foreach (var elem in Sockets)
			{
				elem.Value.send(msg);
			}
		}

		public void SetSocketState(uint socketId, NetworkSocketState networkSocketState)
		{
			SocketRouteStates[socketId].Detach(SocketStates[socketId]);
			SocketRouteStates[socketId].Attach(networkSocketState);
			SocketStates[socketId] = networkSocketState;
		}
	}
}
