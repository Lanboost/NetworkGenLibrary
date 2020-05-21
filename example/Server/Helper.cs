using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Helper
	{
		public static Lobby GetLobbyByConnectionId(Program p, uint connectionId)
		{
			return p.Lobbies[p.Connections[connectionId].LobbyId];
		}

	}
}
