using LNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGameGateNetwork
{
    public class ServerAPI
    {
		//lobby

		public static void SendLobby(int cid)
		{
			return PacketBuilder.New().AddInt32(PacketIds.SERVER_LOBBY).Build();
		}

		public static void SendLobbyUpdate(int cid)
		{

		}

		//game

		public static void SendEnterGame(int cid)
		{
			
		}

		public static void SendLeaveGame(int cid)
		{

		}

		public static void SendKickedFromGame(int cid)
		{

		}

		//party
		public static void SendPartyInvite(int cid)
		{

		}

		public static void SendPartyAccept(int cid)
		{

		}

		public static void SendPartyDecline(int cid)
		{

		}

		public static void SendPartyLeave(int cid)
		{

		}
		public static void SendPartyKicked(int cid)
		{

		}

	}
}
