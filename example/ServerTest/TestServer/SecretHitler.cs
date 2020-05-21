using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;

namespace TestServer
{
	class TestHelper
	{
		public static void WrapAction<T>(uint id, RPC<T> rpc, T value)
		{

			rpc.SetCallback(delegate (byte[] arr)
			{
				var mem = new MemoryStream(arr);
				BinaryReader reader = new BinaryReader(mem);
				var rpcid = reader.ReadInt32();

				rpc.Read(id, reader);

			});
			rpc.Call(value);
		}
	}

	public class SecretHitlerGameStateHelper
	{
		Random r = new Random();

		uint[] connections;
		string[] names;

		public SecretHitlerGameStateHelper(uint[] connections, string[] names)
		{
			this.connections = connections;
			this.names = names;
		}

		public SecretHitlerGame GetStartState()
		{
			RPCHandler handler = new RPCHandler();

			SecretHitlerGame game = new SecretHitlerGame(handler);

			game.Init();
			game.OnStart(connections, names);
			game.OnTick();

			return game;
		}

		public SecretHitlerGame GetSelectCancellorState()
		{
			return GetStartState();
		}

		public SecretHitlerGame GetSelectCancellorVoteState()
		{
			var game = GetSelectCancellorState();
			game.Cancellor = r.Next(1, connections.Length+1);
			game.State = SecretHitlerGame.STATE_SELECT_CANCELLOR_VOTE;
			return game;
		}

		public SecretHitlerGame GetPresidentDiscardCardState()
		{
			var game = GetSelectCancellorState();
			game.Cancellor = r.Next(1, connections.Length + 1);
			game.State = SecretHitlerGame.STATE_PRESIDENT_DISCARD_CARD;

			game.Hand.Clear();
			for (int i = 0; i < 3; i++)
			{
				game.Hand.Add(game.Deck.Dequeue());
			}

			return game;
		}

		public SecretHitlerGame GetCancellorDiscardCardState()
		{
			var game = GetPresidentDiscardCardState();

			game.State = SecretHitlerGame.STATE_CANCELLOR_DISCARD_CARD;

			game.Discard.Enqueue(game.Hand[1]);
			game.Hand.RemoveAt(1);
			return game;
		}

		public SecretHitlerGame GetLastPresidentAndCancellorSetGameState()
		{
			return null;
		}

		public SecretHitlerGame GetPresidentInspectPlayerGameState()
		{
			return null;
		}

		public SecretHitlerGame GetPresidentKillPlayerGameState()
		{
			return null;
		}

	}


	[TestClass]
	public class SecretHitlerGameTest
	{
		[TestMethod]
		public void TestPresidentCanSelectCancellor()
		{
			var connections = new uint[] { 0, 1, 2 };
			var names = new string[] { "0", "1", "2" };

			var helper = new SecretHitlerGameStateHelper(connections, names);
			var game = helper.GetSelectCancellorState();

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, -1);

			TestHelper.WrapAction(0, game.SelectCancellor, 1);

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, 1);

		}

		[TestMethod]
		public void TestOnlyPresidentCanSelectCancellor()
		{
			var connections = new uint[] { 0, 1, 2 };
			var names = new string[] { "0", "1", "2" };

			var helper = new SecretHitlerGameStateHelper(connections, names);
			var game = helper.GetSelectCancellorState();

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, -1);

			TestHelper.WrapAction(1, game.SelectCancellor, 1);

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, -1);

		}

		[TestMethod]
		public void TestCancellorVoteFail()
		{
			var connections = new uint[] { 0, 1, 2 ,3 };
			var names = new string[] { "0", "1", "2", "3" };

			var helper = new SecretHitlerGameStateHelper(connections, names);
			var game = helper.GetSelectCancellorState();

			TestHelper.WrapAction(0, game.SelectCancellor, 1);
			TestHelper.WrapAction(0, game.Vote, true);
			TestHelper.WrapAction(1, game.Vote, true);
			TestHelper.WrapAction(2, game.Vote, false);
			TestHelper.WrapAction(3, game.Vote, false);

			Assert.AreEqual(game.President, 1);
			Assert.AreEqual(game.Cancellor, -1);

		}

		[TestMethod]
		public void TestCancellorVoteSuccess()
		{
			var connections = new uint[] { 0, 1, 2 };
			var names = new string[] { "0", "1", "2" };

			var helper = new SecretHitlerGameStateHelper(connections, names);
			var game = helper.GetSelectCancellorState();

			TestHelper.WrapAction(0, game.SelectCancellor, 1);
			TestHelper.WrapAction(0, game.Vote, true);
			TestHelper.WrapAction(1, game.Vote, true);

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, 1);
		}


		[TestMethod]
		public void TestDiscardCard()
		{
			RPCHandler handler = new RPCHandler();

			SecretHitlerGame game = new SecretHitlerGame(handler);

			var connections = new uint[] { 0, 1, 2 };
			var names = new string[] { "0", "1", "2" };

			game.Init();
			game.OnStart(connections, names);
			game.OnTick();
			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, -1);

			TestHelper.WrapAction(0, game.SelectCancellor, 1);

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, 1);

			TestHelper.WrapAction(0, game.Vote, true);
			TestHelper.WrapAction(1, game.Vote, true);

			Assert.AreEqual(game.President, 0);
			Assert.AreEqual(game.Cancellor, 1);
		}
	}
}
