using LNetwork.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.plugins.auth
{
	public class AuthenticationClientPlugin : NetworkSocketState
	{
		IRPCNetworkSocketState socketNetwork;
		Func<NetworkSocketHandler, uint, LoginPacket, Action<LoginResponsePacket>, uint> loginRPC;
		Action<LoginResponsePacket> loginCallback;

		public AuthenticationClientPlugin(NetworkPacketIdGenerator generator, IRPCNetworkSocketState socketNetwork, Action<LoginResponsePacket> loginCallback)
		{
			this.loginCallback = loginCallback;
			loginRPC = socketNetwork.RegisterRPC<LoginPacket, LoginResponsePacket>(generator.Register());
		}

		public void Handle(NetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			socketNetwork.Handle(handler, socketId, packetId, reader);
		}

		public uint[] PacketIdList()
		{
			return socketNetwork.PacketIdList();
		}

		public void Login(NetworkSocketHandler handler, string username, string password)
		{
			loginRPC.Invoke(handler, uint.MaxValue, new LoginPacket(username, password), loginCallback);
		}


	}

	public class AuthenticationServerPlugin : NetworkSocketState
	{
		IRPCNetworkSocketState socketNetwork;
		public AuthenticationServerPlugin(NetworkPacketIdGenerator generator, IRPCNetworkSocketState socketNetwork, Func<uint, uint, LoginPacket, LoginResponsePacket> handler)
		{
			socketNetwork.RegisterRPCHandler<LoginPacket, LoginResponsePacket>(generator.Register(), handler);
		}

		public void Handle(NetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			socketNetwork.Handle(handler, socketId, packetId, reader);
		}

		public uint[] PacketIdList()
		{
			return socketNetwork.PacketIdList();
		}
	}
}