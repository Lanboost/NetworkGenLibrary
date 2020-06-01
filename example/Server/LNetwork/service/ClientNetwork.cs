using LNetwork.normal;
using LNetwork.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{

	public class PingNetworkState : NetworkSocketState
	{
		StandardRPCNetworkSocketState rpcState;
		Func<uint, PingPacket, Action<PingResponse>, uint> pingRpc;

		public PingNetworkState(INetworkSocketHandler handler, uint sendPacketId, uint respondPacketId, uint pollTime = 1000*30, uint timeout = 1000*60)
		{
			rpcState = new StandardRPCNetworkSocketState();
			rpcState.Bind(handler);
			pingRpc = rpcState.RegisterRPCDual<PingPacket, PingResponse>(sendPacketId, respondPacketId, 
				delegate(uint socketId, uint packetId, PingPacket pingPacket) {

					
					return new PingResponse();
				});
		}

		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			rpcState.Handle(handler, socketId, packetId, reader);
		}

		public uint[] PacketIdList()
		{
			return rpcState.PacketIdList();
		}
	}

	public class EnableCloseNetworkState : NetworkSocketState
	{
		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public uint[] PacketIdList()
		{
			throw new NotImplementedException();
		}
	}

	public class ClientNetwork: INetworkSocketHandler, INetworkSocketHandlerClient
	{
		ClientSocket ClientSocket;
		DataSocket socket;

		NetworkSocketStateRouter router;
		IBuilder<ClientSocket> clientSocketBuilder;

		public ClientNetwork(IBuilder<ClientSocket> clientSocketBuilder)
		{
			router = new NetworkSocketStateRouter();

			this.clientSocketBuilder = clientSocketBuilder;

		}

		public void Connect(string host, int port)
		{
			ClientSocket = this.clientSocketBuilder.Build();
			ClientSocket.connect(host, port);
		}

		public DataSocket GetSocket()
		{
			return socket;
		}
		
		public NetworkSocketStateRouter GetSocketRouter()
		{
			return router;
		}

		public void Handle()
		{
			if (socket != null)
			{
				socket.handle();

				while(true)
				{
					var msg = socket.getMessage();
					if(msg != null)
					{
						BinaryReader reader = new BinaryReader(new MemoryStream(msg));
						uint cmd = reader.ReadUInt32();
						router.Handle(this, 0, cmd, reader);
					}
					else
					{
						break;
					}
				}
			}
			else
			{
				if (ClientSocket != null)
				{
					socket = ClientSocket.handle();
				}
			}
		}

		public void Send(byte[] msg)
		{
			socket.send(msg);
		}

		public void Send(uint socketId, byte[] msg)
		{
			socket.send(msg);
		}
	}
}
