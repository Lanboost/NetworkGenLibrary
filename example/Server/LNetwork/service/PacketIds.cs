using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.service
{
	public class PacketIds
	{
		public static int PACKET_ID_CLIENT_LOGIN = 0;
		public static int PACKET_ID_CLIENT_ACCEPT_CLOSE = 0;
		public static int PACKET_ID_CLIENT_PING = 1;

		public static int PACKET_ID_SERVER_CLOSE = 0;
		public static int PACKET_ID_SERVER_AUTH = 1;
	}
}
