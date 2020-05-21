using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	

	public class Game
	{
		public uint GameId
		{
			get; set;
		}

		public GameInstance GameInstance
		{
			get; set;
		}

		public RPCHandler RPCHandler
		{
			get; set;
		}

		public List<uint> Players
		{
			get; set;
		}

		public Game(List<uint> players)
		{
			Players = players;
		}
	}

	
}
