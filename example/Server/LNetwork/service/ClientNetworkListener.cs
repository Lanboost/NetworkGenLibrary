using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public abstract class ClientNetworkListener
	{
		public abstract void OnConnected();

		public abstract void OnAuthenticated();

		public abstract void OnDisconnected(string reason);

		public abstract void OnError(string reason);

		public abstract void OnMessage(byte[] msg);
	}
}
