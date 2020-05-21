

using System.Collections.Generic;
namespace LNetwork
{
	public interface NetworkPacketIdGenerator
	{
		uint Register();
		void Unregister(uint packetId);
	}


	public class StandardNetworkPacketIdGenerator : NetworkPacketIdGenerator
	{
		Dictionary<uint, bool> UsedPackets = new Dictionary<uint, bool>();
		UIDCounter PacketIdCounter = new UIDCounter();

		public uint Register()
		{
			var p = PacketIdCounter.Get();
			UsedPackets.Add(p, true);
			return p;
		}

		public void Unregister(uint packetId)
		{
			UsedPackets.Remove(packetId);
		}
	}
}