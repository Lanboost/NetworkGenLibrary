using LNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Connection
	{

		public DataSocket DataSocket
		{
			get; set;
		}

		public uint LobbyId
		{
			get; set;
		}


		public uint GameId
		{
			get; set;
		}

		public uint ConnectionId
		{
			get; set;
		}

		public uint State
		{
			get; set;
		}

		public string Username
		{
			get; set;
		}

		public uint AccountId
		{
			get; set;
		}

		public uint LastCommand
		{
			get; set;
		}

		public bool Error
		{
			get; set;
		}

		public string ErrorMessage
		{
			get; set;
		}

		public Connection(uint connectionId, DataSocket dataSocket)
		{
			DataSocket = dataSocket;
			ConnectionId = connectionId;
		}
	}
}
