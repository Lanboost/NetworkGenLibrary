using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGameGateNetwork
{
	abstract class ServerRecListener
	{
		public abstract void OnConnected(int cid);

		public abstract void OnLogin(int cid, string username, string password);

		public abstract void OnJoinGame(int cid);

		public abstract void OnLeaveGame(int cid);

		public abstract void OnJoinParty(int cid);

		public abstract void OnLeaveParty(int cid);
	}
}
