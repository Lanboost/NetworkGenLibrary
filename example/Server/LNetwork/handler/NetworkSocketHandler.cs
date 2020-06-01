
using LNetwork.service;
using System;
using System.Collections.Generic;

namespace LNetwork
{

	public interface INetworkSocketHandler
	{
		void Handle();
		void Send(uint socketId, byte[] msg);
	}

	public interface INetworkSocketHandlerServer: INetworkSocketHandler
	{
		IEnumerable<Tuple<uint, DataSocket>> GetSockets();
		int GetSocketCount();
		DataSocket GetSocket(uint socketId);
		NetworkSocketStateRouter GetSocketRouter(uint socketId);

		void BroadCast(byte[] msg);
	}

	public interface INetworkSocketHandlerClient: INetworkSocketHandler
{
		DataSocket GetSocket();
		NetworkSocketStateRouter GetSocketRouter();
		void Send(byte[] msg);
	}

}