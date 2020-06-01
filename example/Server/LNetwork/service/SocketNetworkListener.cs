using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public abstract class SocketNetworkListener
	{
		public abstract void OnConnected(uint socketId);

		public abstract void OnDisconnected(uint socketId, string reason);

		public abstract void OnError(uint socketId, string reason);
	}
}
