using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	public class Client
	{

		public static int STATE_AWAIT_LOGIN = 0;
		public static int STATE_CONNECTING = 1;
		public static int STATE_AUTHENTICATING = 2;
		public static int STATE_LOBBY = 3;

		public int State
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

		public string Username
		{
			get; set;
		}

		public uint ConnectionId
		{
			get; set;
		}

		public GameLobby GameLobby
		{
			get; set;
		}

		public ClientSocket socket;
		public DataSocket dataSocket;
		private byte[] connectData;
		RPCHandler rpc;
		GameInstance game;
		Player[] players;

		// Update is called once per frame
		void Update()
		{
			if (socket != null && dataSocket == null)
			{
				var dsocket = socket.handle();
				if (dsocket != null)
				{
					this.dataSocket = dsocket;
					dataSocket.send(connectData);
					connectData = null;
				}
			}

			if (dataSocket != null)
			{
				dataSocket.handle();

				var msg = dataSocket.getMessage();
				if (msg != null)
				{
					handleMessage(msg);
				}
			}
		}

		public void Connect(string username, string password)
		{
			//connect data
			var mem = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(mem);
			writer.Write((System.Int16)0);
			writer.Write(username);
			writer.Write(password);
			writer.Flush();
			this.connectData = mem.ToArray();


			State = STATE_CONNECTING;
			socket = new NetSocketClientSocket();
			socket.connect("127.0.0.1", 8888);
			
			this.connectData = mem.ToArray();
		}

		public void handleMessage(byte[] msg)
		{
			BinaryReader reader = new BinaryReader(new MemoryStream(msg));

			int cmd = reader.ReadInt16();
			if (cmd == 0)
			{
				bool ok = reader.ReadBoolean();
				if (ok)
				{
					ConnectionId = reader.ReadUInt32();
					this.Username = reader.ReadString();
					SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
				}
				else
				{
					this.State = 0;
					this.Error = true;
					this.ErrorMessage = reader.ReadString();
				}
			}
			else if (cmd == 2)
			{
				var connId = reader.ReadUInt32();
				if (connId == ConnectionId)
				{
					this.State = 2;
					GameLobby = new GameLobby();
					GameLobby.Players.Add(connId, new GameLobbyPlayer(connId, reader.ReadString()));
					SceneManager.LoadScene("GameLobbyScene", LoadSceneMode.Single);
				}
				else
				{
					GameLobby.Players.Add(connId, new GameLobbyPlayer(connId, reader.ReadString()));
				}
			}
			else if (cmd == 3)
			{
				var connId = reader.ReadUInt32();
				GameLobby.HostId = connId;
			}
			else if (cmd == 4)
			{
				var connId = reader.ReadUInt32();
				if (connId == ConnectionId)
				{
					State = 1;
					GameLobby = null;
				}
				else
				{
					GameLobby.Players.Remove(connId);
				}
			}

			else if (cmd == 5)
			{
				var gameId = reader.ReadUInt32();

				RPCHandler RPCHandler = new RPCHandler();
				game = new SecretHitlerGame(RPCHandler);
				FindObjectOfType<GameData>().setGameData(game);
				game.Init();

				uint[] ids = new uint[this.GameLobby.Players.Count];
				string[] names = new string[this.GameLobby.Players.Count];

				int index = 0;
				foreach (var p in this.GameLobby.Players.Values)
				{
					ids[index] = p.ConnectionId;
					names[index] = p.Username;
					index++;
				}

				game.OnStart(ids, names);
				SceneManager.LoadScene("SecretHitler", LoadSceneMode.Single);

			}
			else if (cmd == 6)
			{
				//RPC
			}
			else if (cmd == 7)
			{
				//tick
				game.OnTick();
				Debug.Log("Tick");
			}
		}
	}
}
