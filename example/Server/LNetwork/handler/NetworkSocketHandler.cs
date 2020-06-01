
using LNetwork.service;
using System;
using System.Collections.Generic;

namespace LNetwork
{

	public interface INetworkSocketHandler
	{

		void Handle();

		IEnumerable<Tuple<uint, DataSocket>> GetSockets();
		int GetSocketCount();
		DataSocket GetSocket(uint socketId);
		NetworkSocketState GetSocketState(uint socketId);

		void Send(uint socketId, byte[] msg);
		void Send(byte[] msg);
		void BroadCast(byte[] msg);

		void SetSocketState(uint socketId, NetworkSocketState networkSocketState);
	}

}