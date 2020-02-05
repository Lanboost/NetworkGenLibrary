using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkTest
{
	[TestClass]
	public class UnitTest1
	{

		[TestInitialize]
		public void TestInitialize()
		{
			NetServer.Main.players.Clear();
			NetServer.Main.sockets.Clear();
		}

		[TestMethod]
		public void TestBaseSend()
		{
			var pb = new NetServer.PlayerBuilder();
			var p = pb.build();
			p.InitName("");

			var mem = new MemoryStream();

			p.Write(null, new BinaryWriter(mem));

			mem.Position = 0;

			var netreader = new NetClient.NetReader();
			netreader.ReadTick(new BinaryReader(mem));

			Assert.IsTrue(netreader.netObjects.Count == 1);
			AssertPlayerEqual(p, netreader.Get<NetClient.Player>(0));

		}

		public void AssertPlayerEqual(NetServer.Player expected, NetClient.Player actual, bool ignoreInventory = true, bool expectNullInventory = false)
		{
			Assert.AreEqual(expected.GetName(), actual.GetName());
			if (expected.GetPosition() == null)
			{
				Assert.IsNull(actual.GetPosition());
			}
			else
			{
				Assert.IsNotNull (actual.GetPosition());
				Assert.AreEqual(expected.GetPosition().GetX(), actual.GetPosition().GetX());
				Assert.AreEqual(expected.GetPosition().GetY(), actual.GetPosition().GetY());
				Assert.AreEqual(expected.GetPosition().GetZ(), actual.GetPosition().GetZ());
			}
			
			Assert.AreEqual(expected.GetHealth(), actual.GetHealth());
			Assert.AreEqual(expected.GetMaxHealth(), actual.GetMaxHealth());
			Assert.AreEqual(expected.GetMana(), actual.GetMana());
			Assert.AreEqual(expected.GetMaxMana(), actual.GetMaxMana());
			Assert.AreEqual(expected.GetName(), actual.GetName());
			Assert.AreEqual(expected.GetName(), actual.GetName());

			if (!ignoreInventory)
			{
				if (expectNullInventory)
				{
					Assert.IsNull(actual.GetInventory());
				}
				else
				{
					if (expected.GetInventory() == null)
					{
						Assert.IsNull(actual.GetInventory());
					}
					else
					{
						Assert.IsNotNull(actual.GetInventory());
						Assert.AreEqual(expected.GetInventory().GetCoins(), actual.GetInventory().GetCoins());

					}
				}
			}

		}


		[TestMethod]
		public void TestBaseSet()
		{

			var sock1 = new NetServer.Socket();
			sock1.pid = 1;

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;

			var p1 = new NetServer.Player();
			p1.netId = 10;
			p1.InitName("");
			p1.InitSocket(sock1);
			p1.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));

			var p2 = new NetServer.Player();
			p2.InitName("");
			p2.InitSocket(sock2);
			p2.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));

			NetServer.Main.players.Add(sock1.pid, p1);
			NetServer.Main.players.Add(sock2.pid, p2);




			var netreader = new NetClient.NetReader();		


			var mem = new MemoryStream();
			p1.Write(null, new BinaryWriter(mem));
			mem.Position = 0;
			netreader.ReadTick(new BinaryReader(mem));
			
			p1.SetName("Ian");


			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));

			Assert.IsTrue(netreader.netObjects.Count == 1);
			AssertPlayerEqual(p1, netreader.Get<NetClient.Player>(p1.netId));

		}

		[TestMethod]
		public void TestFullSync()
		{
			var sock1 = new NetServer.Socket();
			sock1.pid = 1;

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;

			var p1 = new NetServer.Player();
			p1.netId = 10;
			p1.InitName("");
			p1.InitSocket(sock1);
			p1.InitPosition(new NetServer.Position(100,100,100,100,100,100));

			var p2 = new NetServer.Player();
			p2.netId = 11;
			p2.InitName("");
			p2.InitSocket(sock2);
			p2.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));

			NetServer.Main.players.Add(sock1.pid, p1);
			NetServer.Main.players.Add(sock2.pid, p2);

			p2.Write(sock2, sock2.tickStream);
			var netreader = new NetClient.NetReader();
			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			

			//move player 2 close to player 1

			//p2.SetPosition(new NetServer.Position(100,1,100,-100,-100,-100));
			p2.SetPosition(new NetServer.Position(-100, 1, 100, -100, -100, -100));
			p2.GetPosition().SetX(100);


			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));

			Assert.AreEqual(2, netreader.netObjects.Count);
			AssertPlayerEqual(p1, netreader.Get<NetClient.Player>(p1.netId));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId));

			p1.SetName("Ian");


			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));

			Assert.AreEqual(2, netreader.netObjects.Count);
			AssertPlayerEqual(p1, netreader.Get<NetClient.Player>(p1.netId));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId));

		}




		[TestMethod]
		public void TestArrayDefaultRead()
		{

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;


			var p2 = new NetServer.Player();
			p2.netId = 11;
			p2.InitName("");
			p2.InitSocket(sock2);
			p2.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));
			p2.InitInventory(new NetServer.Inventory(new NetServer.Item[] { new NetServer.Item(0,0,1), new NetServer.Item(0, 0, 1) }, 111));

			NetServer.Main.players.Add(sock2.pid, p2);

			p2.Write(sock2, sock2.tickStream);
			var netreader = new NetClient.NetReader();
			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);
		}

		[TestMethod]
		public void TestArrayChangeArray()
		{

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;


			var p2 = new NetServer.Player();
			p2.netId = 11;
			p2.InitName("");
			p2.InitSocket(sock2);
			p2.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));
			p2.InitInventory(new NetServer.Inventory(new NetServer.Item[] { new NetServer.Item(0, 0, 1), new NetServer.Item(0, 0, 1) }, 111));

			NetServer.Main.players.Add(sock2.pid, p2);

			p2.Write(sock2, sock2.tickStream);
			var netreader = new NetClient.NetReader();
			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);


			p2.SetInventory(new NetServer.Inventory(new NetServer.Item[] { new NetServer.Item(0, 0, 1), new NetServer.Item(0, 0, 1) }, 222));

			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);
		}

		[TestMethod]
		public void TestArrayChangeArrayItem()
		{

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;


			var p2 = new NetServer.Player();
			p2.netId = 11;
			p2.InitName("");
			p2.InitSocket(sock2);
			p2.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));
			p2.InitInventory(new NetServer.Inventory(new NetServer.Item[] { new NetServer.Item(0, 0, 1), new NetServer.Item(0, 0, 1) }, 111));

			NetServer.Main.players.Add(sock2.pid, p2);

			p2.Write(sock2, sock2.tickStream);
			var netreader = new NetClient.NetReader();
			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);


			p2.GetInventory().GetItems()[0].SetCount(2);

			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);
		}

		[TestMethod]
		public void TestArrayAssignArrayItem()
		{

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;


			var p2 = new NetServer.Player();
			p2.netId = 11;
			p2.InitName("");
			p2.InitSocket(sock2);
			p2.InitPosition(new NetServer.Position(0, 0, 0, 0, 0, 0));
			p2.InitInventory(new NetServer.Inventory(new NetServer.Item[] { new NetServer.Item(0, 0, 1), new NetServer.Item(0, 0, 1) }, 111));

			NetServer.Main.players.Add(sock2.pid, p2);

			p2.Write(sock2, sock2.tickStream);
			var netreader = new NetClient.NetReader();
			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);


			p2.GetInventory().GetItems()[0] = new NetServer.Item(1, 1, 1);

			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));
			AssertPlayerEqual(p2, netreader.Get<NetClient.Player>(p2.netId), false, false);
		}
	}
}
