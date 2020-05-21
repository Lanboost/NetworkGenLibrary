using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.service
{
	public abstract class ServerNetworkListener
	{
		public abstract void OnConnected(int cid);

		public abstract ServerNetworkLoginResponse OnLogin(int cid, string username, string password);

		public abstract void OnDisconnected(int cid);

		public abstract void OnError(int cid);

		public abstract void OnMessage(int cid, byte[] msg);
	}
}
