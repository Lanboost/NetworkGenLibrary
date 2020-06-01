﻿using LNetwork.normal;
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
		IRPCNetworkSocketState rpcState;
		Func<INetworkSocketHandler, uint, PingPacket, Action<PingResponse>, uint> pingRpc;

		public PingNetworkState(NetworkPacketIdGenerator gen, uint pollTime = 1000*30, uint timeout = 1000*60)
		{
			rpcState = new StandardRPCNetworkSocketState();
			pingRpc = rpcState.RegisterRPCDual<PingPacket, PingResponse>(gen.Register(), gen.Register(), 
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

	public class ClientNetwork: INetworkSocketHandler
	{
		ClientSocket ClientSocket;
		DataSocket socket;

		NetworkSocketStateRouter router;
		IBuilder<ClientSocket> clientSocketBuilder;

		NetworkSocketState state;

		public ClientNetwork(IBuilder<ClientSocket> clientSocketBuilder, NetworkPacketIdGenerator gen)
		{
			//Wrap socket in a RPC ping handler
			router = new NetworkSocketStateRouter();
			var pingState = new PingNetworkState(gen);
			router.Attach(pingState);

			this.clientSocketBuilder = clientSocketBuilder;

		}

		public void BroadCast(byte[] msg)
		{
			throw new NotImplementedException();
		}

		public void Connect(string host, int port)
		{
			ClientSocket = this.clientSocketBuilder.Build();
			ClientSocket.connect(host, port);
		}

		public DataSocket GetSocket(uint socketId)
		{
			throw new NotImplementedException();
		}

		public int GetSocketCount()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Tuple<uint, DataSocket>> GetSockets()
		{
			throw new NotImplementedException();
		}

		public NetworkSocketState GetSocketState(uint socketId)
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
			Send(msg);
		}

		public void SetSocketState(uint socketId, NetworkSocketState networkSocketState)
		{
			if(state != null)
			{
				router.Detach(state);
			}
			this.state = networkSocketState;
			if (state != null)
			{
				router.Attach(state);
			}
		}
	}
}
