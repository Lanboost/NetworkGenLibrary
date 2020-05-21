using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class LobbyController
	{

		public static void CreateLobby(Program p, uint connectionId, string name, uint game)
		{
			if (ConnectionState.EnsureConnectionAuthenticated(p, connectionId) && ConnectionState.EnsureConnectionNotInLobby(p, connectionId) && ConnectionState.EnsureConnectionNotInGame(p, connectionId))
			{
				Lobby l = new Lobby();

				uint id = p.LobbyIdGenerator.Next();
				l.LobbyId = id;
				p.Lobbies.Add(l.LobbyId, l);

				AddToLobby(p, connectionId, id);
				SetHost(p, connectionId);

				//Send new lobby to clients
				foreach (var c in p.Connections.Values)
				{
					if (c.State == ConnectionState.Authenticated && c.LobbyId == uint.MaxValue && c.GameId == uint.MaxValue)
					{

						var mem = new MemoryStream();
						BinaryWriter writer = new BinaryWriter(mem);

						writer.Write((System.Int16)1);
						writer.Write((System.Int16)2);
						writer.Write((System.UInt32)1);

						writer.Write((System.UInt32)l.LobbyId);
						writer.Write(l.Name);
						writer.Write((System.UInt32)l.GameType);
						writer.Flush();

						c.DataSocket.send(mem.ToArray());
					}
				}
			}
			else
			{
				Console.WriteLine($"Error from socket, assert failed");
			}
		}

		public static void StartLobby(Program p, uint connectionId)
		{
			if (ConnectionState.EnsureConnectionInLobby(p, connectionId) && ConnectionState.EnsureConnectionLobbyHost(p, connectionId))
			{
				var lobby = Helper.GetLobbyByConnectionId(p, connectionId);

				var game = new Game(lobby.Players);
				game.GameId = p.GameIdGenerator.Next();
				p.Games.Add(game.GameId, game);

				game.RPCHandler = new RPCHandler();
				game.GameInstance = new SecretHitlerGame(game.RPCHandler);

				

				//update clients
				foreach (var playerIds in p.Lobbies[lobby.LobbyId].Players)
				{

					var mem = new MemoryStream();
					BinaryWriter writer = new BinaryWriter(mem);

					writer.Write((System.Int16)5);
					writer.Write((System.UInt32)game.GameId);
					writer.Flush();

					p.Connections[playerIds].DataSocket.send(mem.ToArray());

					p.Connections[playerIds].LobbyId = uint.MaxValue;
					p.Connections[playerIds].GameId = game.GameId;
				}

				game.GameInstance.Init();

				string[] names = new string[p.Lobbies[lobby.LobbyId].Players.Count];
				for (int i = 0; i < p.Lobbies[lobby.LobbyId].Players.Count; i++)
				{
					names[i] = p.Connections[p.Lobbies[lobby.LobbyId].Players[i]].Username;
				}

				game.GameInstance.OnStart(p.Lobbies[lobby.LobbyId].Players.ToArray(), names);

				DeleteLobby(p, lobby.LobbyId);
			}
		}

		public static void JoinLobby(Program p, uint connectionId, uint lobbyId)
		{
			if (ConnectionState.EnsureConnectionAuthenticated(p, connectionId) && ConnectionState.EnsureConnectionNotInLobby(p, connectionId) && ConnectionState.EnsureConnectionNotInGame(p, connectionId))
			{
				if (p.Lobbies.ContainsKey(lobbyId))
				{
					AddToLobby(p, connectionId, lobbyId);
				}
				else
				{

				}
			}
		}

		public static void LeaveLobby(Program p, uint connectionId)
		{
			if (ConnectionState.EnsureConnectionInLobby(p, connectionId))
			{
				RemoveFromLobby(p, connectionId);
			}
		}

		public static void PassHost(Program p, uint connectionId, uint newHostId)
		{
			if (ConnectionState.EnsureConnectionInLobby(p, connectionId))
			{
				var lobby = Helper.GetLobbyByConnectionId(p, connectionId);
				if (lobby.Players.Contains(newHostId))
				{
					SetHost(p, newHostId);
				}
			}
		}


		//INTERNAL methods

		public static void AddToLobby(Program p, uint connectionId, uint lobbyId)
		{
			p.Connections[connectionId].LobbyId = lobbyId;

			p.Lobbies[lobbyId].Players.Add(connectionId);

			//update clients

			foreach (var playerIds in p.Lobbies[lobbyId].Players)
			{
				Console.WriteLine($"Sending lobby to:{playerIds}");
				var mem = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(mem);

				writer.Write((System.Int16)2);
				writer.Write((System.UInt32)connectionId);
				writer.Write(p.Connections[playerIds].Username);
				writer.Flush();

				p.Connections[playerIds].DataSocket.send(mem.ToArray());
			}
		}

		public static void SetHost(Program p, uint connectionId)
		{
			var lobby = Helper.GetLobbyByConnectionId(p, connectionId);

			p.Lobbies[lobby.LobbyId].HostId = connectionId;

			//update clients
			foreach (var playerIds in p.Lobbies[lobby.LobbyId].Players)
			{

				var mem = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(mem);

				writer.Write((System.Int16)3);
				writer.Write((System.UInt32)connectionId);
				writer.Flush();

				p.Connections[playerIds].DataSocket.send(mem.ToArray());
			}
		}

		public static void RemoveFromLobby(Program p, uint connectionId)
		{
			var lobby = Helper.GetLobbyByConnectionId(p, connectionId);

			p.Connections[connectionId].LobbyId = uint.MaxValue;

			//update clients
			foreach (var playerIds in p.Lobbies[lobby.LobbyId].Players)
			{

				var mem = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(mem);

				writer.Write((System.Int16)4);
				writer.Write((System.UInt32)connectionId);
				writer.Flush();

				p.Connections[playerIds].DataSocket.send(mem.ToArray());
			}

			lobby.Players.Remove(connectionId);
			if (lobby.Players.Count == 0)
			{
				DeleteLobby(p, lobby.LobbyId);
			}
			else
			{
				if (lobby.HostId == connectionId)
				{
					SetHost(p, lobby.Players[0]);
				}
			}

			//send lobbies to player
			
		}

		public static void DeleteLobby(Program p, uint lobbyId)
		{

			p.Lobbies.Remove(lobbyId);

			//update clients

			foreach (var c in p.Connections.Values)
			{
				if (c.State == ConnectionState.Authenticated && c.LobbyId == uint.MaxValue && c.GameId == uint.MaxValue)
				{

					var mem = new MemoryStream();
					BinaryWriter writer = new BinaryWriter(mem);

					writer.Write((System.Int16)1);
					writer.Write((System.Int16)3);
					writer.Write((System.UInt32)1);
					writer.Write((System.UInt32)lobbyId);
					writer.Flush();

					c.DataSocket.send(mem.ToArray());
				}
			}
		}
	}
}
