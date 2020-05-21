using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.plugins.auth
{
	[Serializable]
	public struct LoginPacket
	{
		string username;
		string password;

		public LoginPacket(string username, string password)
		{
			this.username = username;
			this.password = password;
		}
	}

	[Serializable]
	public struct LoginResponsePacket
	{
		bool success;
		string failureReason;
	}
}
