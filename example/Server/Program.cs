using LNetwork;
using LNetwork.normal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	public class Program
	{

		ServerSocket serverSocket;


		public IdGenerator ConnectorIdGenerator
		{
			get; set;
		}

		public Dictionary<uint, Connection> Connections
		{
			get; set;
		}


		public IdGenerator LobbyIdGenerator
		{
			get; set;
		}

		public Dictionary<uint, Lobby> Lobbies
		{
			get; set;
		}

		public IdGenerator GameIdGenerator
		{
			get; set;
		}

		public Dictionary<uint, Game> Games
		{
			get; set;
		}

		public Program()
		{
			Lobbies = new Dictionary<uint, Lobby>();
			Games = new Dictionary<uint, Game>();
			Connections = new Dictionary<uint, Connection>();

			ConnectorIdGenerator = new IdGenerator();
			LobbyIdGenerator = new IdGenerator();
			GameIdGenerator = new IdGenerator();
		}

		

		static void Main(string[] args)
		{

			var p = new Program();
			p.Start();
		}

		void Start()
		{
			serverSocket = new NetSocketServerSocket();
			serverSocket.listen(8888);
			Console.WriteLine("Listening");
			while (true)
			{
				DataSocket s = serverSocket.handle();
				if (s != null)
				{
					Console.WriteLine("Connection aquired");
					Connection c = new Connection(ConnectorIdGenerator.Next(), s);

					Connections.Add(c.ConnectionId, c);
				}

				foreach (Connection connection in Connections.Values)
				{
					connection.DataSocket.handle();


					var msg = connection.DataSocket.getMessage();
					if (msg != null)
					{
						BinaryReader reader = new BinaryReader(new MemoryStream(msg));

						int cmd = reader.ReadInt16();
						Console.WriteLine($"Got msg command: {cmd}");
						if (cmd == 0)
						{
							if (ConnectionState.EnsureConnectionNotAuthenticated(this, connection.ConnectionId))
							{
								string name = reader.ReadString();
								string password = reader.ReadString();

								connection.State = ConnectionState.Authenticated;
								connection.Username = name;
								connection.LobbyId = uint.MaxValue;
								connection.GameId = uint.MaxValue;

								Console.WriteLine($"Player authenticated id:{connection.ConnectionId} name:{name}");

								{

									var mem = new MemoryStream();
									BinaryWriter writer = new BinaryWriter(mem);

									writer.Write((System.Int16)0);
									writer.Write(true);
									writer.Write((System.UInt32)connection.ConnectionId);
									writer.Write("Player 1");

									writer.Flush();

									connection.DataSocket.send(mem.ToArray());
								}

								//send lobbies
								{

									var mem = new MemoryStream();
									BinaryWriter writer = new BinaryWriter(mem);

									writer.Write((System.Int16)1);
									writer.Write((System.Int16)1);
									writer.Write((System.UInt32)Lobbies.Values.Count);
									foreach (var l in Lobbies.Values)
									{
										writer.Write((System.UInt32)l.LobbyId);
										writer.Write(l.Name);
										writer.Write((System.UInt32)l.GameType);
									}

									writer.Flush();

									connection.DataSocket.send(mem.ToArray());
								}
							}
						}
						else if (cmd == 1)
						{
							string name = reader.ReadString();
							uint game = reader.ReadUInt32();
							LobbyController.CreateLobby(this, connection.ConnectionId, name, game);
						}
						else if (cmd == 2)
						{
							LobbyController.StartLobby(this, connection.ConnectionId);
						}
						else if (cmd == 3)
						{
							uint lobbyId = reader.ReadUInt32();
							LobbyController.JoinLobby(this, connection.ConnectionId, lobbyId);
						}
						else if (cmd == 4)
						{
							LobbyController.LeaveLobby(this, connection.ConnectionId);
						}
						else if (cmd == 5)
						{
							if (connection.GameId != uint.MaxValue)
							{
								Games[connection.GameId].RPCHandler.Read(connection.ConnectionId, reader);
							}
							//Send to all sockets

							{
								Console.WriteLine("Broadcasting rpc");
								var mem = new MemoryStream();
								BinaryWriter writer = new BinaryWriter(mem);

								writer.Write((System.Int16)6);
								writer.Write((System.UInt32)connection.ConnectionId);
								writer.Write(msg, 2, msg.Length - 2);

								writer.Flush();
								
								foreach (var connId in Games[connection.GameId].Players)
								{
									Connections[connId].DataSocket.send(mem.ToArray());
								}
							}
						}
						else
						{
							connection.Error = true;
							connection.ErrorMessage = "Network error";
						}
					}
				}

				foreach (var game in Games.Values)
				{
					/*game.onTick();
					if (game.isDone())
					{
						
					}*/
					//GameController.Handle();

					game.GameInstance.OnTick();
					{
						var mem = new MemoryStream();
						BinaryWriter writer = new BinaryWriter(mem);

						writer.Write((System.Int16)7);
						writer.Flush();

						foreach (var connId in game.Players)
						{
							Connections[connId].DataSocket.send(mem.ToArray());
						}
					}

					if (game.GameInstance.IsDone())
					{
						//Stop game
					}
				}

				Thread.Sleep(200);
			}
		}

		/*void StartGame(Connection[] players)
		{
			Console.WriteLine("Starting game");

			//send start to all players
			{

				var mem = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(mem);

				writer.Write((System.Int16)1);
				writer.Write((System.Int16)players.Length);
				foreach (var c in players)
				{
					writer.Write(c.username);
				}

				writer.Flush();

				foreach (var c in players)
				{
					c.DataSocket.send(mem.ToArray());
				}
			}

			RPCHandler handler = new RPCHandler(delegate (byte[] data)
			{
				throw new Exception("As a server this should never be called!");
			});

			Game g = new Game(handler);
			g.init();

			Player[] plist = new Player[players.Length];
			for (int i = 0; i < players.Length; i++)
			{
				plist[i] = new Player(players[i].username);
			}

			g.onStart(plist);
			while (!g.isDone())
			{
				for (int i = 0; i < players.Length; i++)
				{
					Connection p = players[i];
					p.DataSocket.handle();


					var msg = p.DataSocket.getMessage();
					if (msg != null)
					{

						BinaryReader reader = new BinaryReader(new MemoryStream(msg));

						int cmd = reader.ReadInt16();
						if (cmd == 0)
						{
							handler.Read(i, reader);
							//send to all other clients
							{
								Console.WriteLine("Broadcasting rpc");
								var mem = new MemoryStream();
								BinaryWriter writer = new BinaryWriter(mem);

								writer.Write((System.Int16)2);
								writer.Write((System.Int16)i);
								writer.Write(msg, 2, msg.Length-2);

								writer.Flush();

								foreach (var c in players)
								{
									c.DataSocket.send(mem.ToArray());
								}
							}
						}
					}

				}

				g.onTick();

				//send tick to all players
				{
					//Console.WriteLine("Sending tick");
					var mem = new MemoryStream();
					BinaryWriter writer = new BinaryWriter(mem);

					writer.Write((System.Int16)3);

					writer.Flush();

					foreach (var c in players)
					{
						c.DataSocket.send(mem.ToArray());
					}
				}

				Thread.Sleep(200);
			}

			Console.WriteLine("Game ended");

		}*/
	}
}
