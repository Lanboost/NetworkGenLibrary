
using LNetwork.service;
using System;
using System.Collections.Generic;

namespace LNetwork
{

	public interface INetworkSocketHandler
	{
		void Handle();
		void Send(uint socketId, byte[] msg);
		void Close();
	}

	public interface INetworkSocketHandlerServer: INetworkSocketHandler
	{
		IEnumerable<Tuple<uint, IDataSocket>> GetSockets();
		int GetSocketCount();
		IDataSocket GetSocket(uint socketId);
		NetworkSocketStateRouter GetSocketRouter(uint socketId);

		void BroadCast(byte[] msg);
	}

	public interface INetworkSocketHandlerClient: INetworkSocketHandler
{
		IDataSocket GetSocket();
		NetworkSocketStateRouter GetSocketRouter();
		void Send(byte[] msg);
	}

}