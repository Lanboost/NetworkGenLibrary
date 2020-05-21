using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	public class GameLobbyPlayer
	{
		public uint ConnectionId
		{
			get; set;
		}

		public string Username
		{
			get; set;
		}

		public GameLobbyPlayer(uint connectionId, string username)
		{
			ConnectionId = connectionId;
			Username = username;
		}
	}

	public class GameLobby
	{
		public uint HostId
		{
			get; set;
		}

		public Dictionary<uint, GameLobbyPlayer> Players
		{
			get; set;
		}

		public GameLobby()
		{
			Players = new Dictionary<uint, GameLobbyPlayer>();
		}
	}
}
