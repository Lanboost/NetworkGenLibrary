using LNetwork.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.test
{
	public class MockedDirectDataSocket : DataSocket
	{
		public void SetDirection(MockedDirectDataSocket socket)
		{

		}

		public override void close()
		{
			throw new NotImplementedException();
		}

		public override byte[] getMessage()
		{
			throw new NotImplementedException();
		}

		public override void handle()
		{
			throw new NotImplementedException();
		}

		public override string ip()
		{
			throw new NotImplementedException();
		}

		public override bool isConnected()
		{
			throw new NotImplementedException();
		}

		public override bool isError()
		{
			throw new NotImplementedException();
		}

		public override void send(byte[] message)
		{
			throw new NotImplementedException();
		}

		public override void setError()
		{
			throw new NotImplementedException();
		}
	}

	public class MockedSockets
	{
		public Dictionary<int, MockedServerSocket> servers = new Dictionary<int, MockedServerSocket>();

		static MockedSockets _instance;

		public static MockedSockets Instance()
		{
			if (_instance == null)
			{
				_instance = new MockedSockets();
			}
			return _instance;
		}


	}

	public class MockedClientSocket : ClientSocket
	{
		bool connecting = false;
		int port = 0;



		public void connect(string server, int port)
		{
			connecting = true;
			this.port = port;
		}

		public DataSocket handle()
		{
			if (connecting)
			{
				connecting = false;
				if (MockedSockets.Instance().servers.ContainsKey(port))
				{
					var clientSocket = new MockedDirectDataSocket();
					var serverSocket = new MockedDirectDataSocket();

					clientSocket.SetDirection(serverSocket);
					serverSocket.SetDirection(clientSocket);

					MockedSockets.Instance().servers[port].waitingConnections.Add(serverSocket);
					return clientSocket;
				}
				else
				{
					throw new Exception("Couldnt connect to server");
				}
			}
			return null;
		}

		public bool isError()
		{
			throw new NotImplementedException();
		}
	}

	public class MockedServerSocket : ServerSocket
	{
		public List<DataSocket> waitingConnections = new List<DataSocket>();
		bool isUsed = false;
		int port = 0;

		public void Close()
		{
			MockedSockets.Instance().servers.Remove(port);
		}

		public DataSocket handle()
		{
			if (waitingConnections.Count > 0)
			{
				var curr = waitingConnections[0];
				waitingConnections.RemoveAt(0);
				return curr;
			}

			return null;
		}

		public void listen(int port)
		{
			if (isUsed)
			{
				throw new Exception("Socket alreadt in use");
			}

			MockedSockets.Instance().servers.Add(port, this);
		}
	}

	public class MockedNetworkHelper
	{
		public ServerNetwork GetServer(NetworkPacketIdGenerator gen, Func<NetworkSocketState> stateBuilder)
		{
			return new ServerNetwork(gen, 
				new BuilderWrapper<ServerSocket>(delegate() { return new MockedServerSocket(); }), 
				new BuilderWrapper<NetworkSocketState>(stateBuilder)
			);
		}

		public ClientNetwork GetClient(NetworkPacketIdGenerator gen)
		{
			return new ClientNetwork(
				new BuilderWrapper<ClientSocket>(delegate () { return new MockedClientSocket(); }),
				gen
			);
		}
	}

	public class AutoConnectedNetwork
	{
		List<ClientNetwork> clients = new List<ClientNetwork>();
		ServerNetwork serverSocket;

		MockedNetworkHelper helper = new MockedNetworkHelper();
		public ServerNetwork GetServer(NetworkPacketIdGenerator gen, Func<NetworkSocketState> stateBuilder)
		{
			serverSocket = helper.GetServer(gen, stateBuilder);
			serverSocket.Listen(0);
			foreach(var c in clients)
			{
				c.Connect("", 0);
			}
			return serverSocket;
		}

		public ClientNetwork GetClient(NetworkPacketIdGenerator gen, NetworkSocketState state)
		{
			var c = helper.GetClient(gen);
			c.SetSocketState(0, state);
			if(serverSocket != null)
			{
				c.Connect("", 0);
			}

			clients.Add(c);
			return c;
		}

		
	}
}
