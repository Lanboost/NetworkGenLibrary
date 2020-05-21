
using LNetwork.service;

namespace LNetwork
{

	public interface NetworkSocketHandler
	{

		void Handle();

		SocketNetwork[] GetSockets();
		uint GetSocketCount();
		SocketNetwork GetSocket(uint socketId);
		NetworkSocketState GetSocketState(uint socketId);

		void Send(uint socketId, byte[] msg);
		void Send(byte[] msg);
		void BroadCast(byte[] msg);

		void SetSocketState(uint socketId, NetworkSocketState networkSocketState);
	}

}