using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.service
{
	[Serializable]
	public struct PingPacket
	{
		uint pingId;
	}

	[Serializable]
	public struct PingResponse
	{
		uint pingId;
	}


	[Serializable]
	public struct SocketClose
	{
	}

	[Serializable]
	public struct SocketCloseAck
	{
	}
}
