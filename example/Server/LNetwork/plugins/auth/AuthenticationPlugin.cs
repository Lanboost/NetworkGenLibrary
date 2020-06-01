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
		StandardRPCNetworkSocketState rpcstate;
		Func<uint, LoginPacket, Action<LoginResponsePacket>, uint> loginRPC;
		Action<LoginResponsePacket> loginCallback;

		public AuthenticationClientPlugin(uint authenticationPacketId, Action<LoginResponsePacket> loginCallback)
		{

			rpcstate = new StandardRPCNetworkSocketState();
			this.loginCallback = loginCallback;
			loginRPC = rpcstate.RegisterRPC<LoginPacket, LoginResponsePacket>(authenticationPacketId);
		}

		public void Bind(INetworkSocketHandler socketHandler)
		{
			rpcstate.Bind(socketHandler);
		}

		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			rpcstate.Handle(handler, socketId, packetId, reader);
		}

		public uint[] PacketIdList()
		{
			return rpcstate.PacketIdList();
		}

		public void Login(string username, string password)
		{
			loginRPC.Invoke(0, new LoginPacket(username, password), loginCallback);
		}


	}

	public class AuthenticationServerPlugin : NetworkSocketState
	{
		StandardRPCNetworkSocketState rpcstate;
		public AuthenticationServerPlugin(uint authenticationPacketId, Func<uint, uint, LoginPacket, LoginResponsePacket> handler)
		{
			rpcstate = new StandardRPCNetworkSocketState();
			rpcstate.RegisterRPCHandler<LoginPacket, LoginResponsePacket>(authenticationPacketId, handler);
		}

		public void Bind(INetworkSocketHandler socketHandler)
		{
			rpcstate.Bind(socketHandler);
		}

		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			rpcstate.Handle(handler, socketId, packetId, reader);
		}

		public uint[] PacketIdList()
		{
			return rpcstate.PacketIdList();
		}
	}
}