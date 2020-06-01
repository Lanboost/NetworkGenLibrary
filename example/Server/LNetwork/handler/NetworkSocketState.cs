using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace LNetwork
{

	public interface NetworkSocketState
	{
		uint[] PacketIdList();
		void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader);
	}

	public class NetworkSocketStateRouter: NetworkSocketState
	{
		Dictionary<uint, NetworkSocketState> Routes = new Dictionary<uint, NetworkSocketState>();

		public void Attach(NetworkSocketState state)
		{
			foreach(uint id in state.PacketIdList())
			{
				Routes.Add(id, state);
			}
		}

		public void Detach(NetworkSocketState state)
		{
			foreach (uint id in state.PacketIdList())
			{
				Routes.Remove(id);
			}
		}

		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			if(Routes.ContainsKey(packetId))
			{
				Routes[packetId].Handle(handler, socketId, packetId, reader);
			}
		}

		public uint[] PacketIdList()
		{
			return Routes.Keys.ToArray();
		}
	}
}