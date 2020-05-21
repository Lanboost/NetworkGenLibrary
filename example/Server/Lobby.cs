using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Lobby
	{
		public string Name
		{
			get; set;
		}

		public uint LobbyId
		{
			get; set;
		}

		public uint HostId
		{
			get; set;
		}

		public uint GameType
		{
			get; set;
		}

		public List<uint> Players
		{
			get; set;
		}

		public Lobby()
		{
			Players = new List<uint>();
		}
	}
}
