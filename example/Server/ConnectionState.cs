using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class ConnectionState
	{
		public static uint NotAuthenticated = 0;
		public static uint Authenticated = 1;

		public static bool EnsureConnectionNotAuthenticated(Program p, uint id)
		{
			var connection = p.Connections[id];
			if (connection.State == NotAuthenticated)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

		public static bool EnsureConnectionAuthenticated(Program p, uint id)
		{
			var connection = p.Connections[id];
			if (connection.State == Authenticated)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

		public static bool EnsureConnectionNotInLobby(Program p, uint id)
		{
			var connection = p.Connections[id];
			if (connection.LobbyId == uint.MaxValue)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

		public static bool EnsureConnectionNotInGame(Program p, uint id)
		{
			var connection = p.Connections[id];
			if (connection.GameId == uint.MaxValue)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

		public static bool EnsureConnectionLobbyHost(Program p, uint id)
		{
			var connection = p.Connections[id];
			var lobby = p.Lobbies[connection.LobbyId];
			if (lobby.HostId == id)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

		public static bool EnsureConnectionInLobby(Program p, uint id)
		{
			var connection = p.Connections[id];
			if (connection.LobbyId != uint.MaxValue)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

		public static bool EnsureConnectionInGame(Program p, uint id)
		{
			var connection = p.Connections[id];
			if (connection.GameId != uint.MaxValue)
			{
				return true;
			}

			connection.Error = true;
			connection.ErrorMessage = "Network error";

			return false;
		}

	}
}
