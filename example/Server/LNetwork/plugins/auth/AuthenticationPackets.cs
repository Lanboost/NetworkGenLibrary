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
		public string username;
		public string password;

		public LoginPacket(string username, string password)
		{
			this.username = username;
			this.password = password;
		}
	}

	[Serializable]
	public struct LoginResponsePacket
	{
		public bool success;
		public string failureReason;

		public LoginResponsePacket(bool success, string failureReason = "")
		{
			this.success = success;
			this.failureReason = failureReason;
		}
	}
}
