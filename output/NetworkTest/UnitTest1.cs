using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var pb = new NetServer.PlayerBuilder();
			var p = pb.build();
			p.SetName("");

			var mem = new MemoryStream();

			p.Write(null, new BinaryWriter(mem));

			mem.Position = 0;

			var netreader = new NetClient.NetReader();
			netreader.ReadTick(new BinaryReader(mem));

			Assert.IsTrue(netreader.netObjects.Count == 1);
			Assert.AreEqual(p.GetName(), netreader.Get<NetClient.Player>(0).GetName());

		}


		[TestMethod]
		public void TestMethod2()
		{
			var pb = new NetServer.PlayerBuilder();
			var p = pb.build();
			p.SetName("");
			var netreader = new NetClient.NetReader();

			var sock1 = new NetServer.Socket();
			sock1.pid = 1;
			p.SetSocket(sock1);

			var sock2 = new NetServer.Socket();
			sock2.pid = 2;
			


			var mem = new MemoryStream();
			p.Write(null, new BinaryWriter(mem));
			mem.Position = 0;
			netreader.ReadTick(new BinaryReader(mem));

			NetServer.Main.players.Add(sock2);
			p.SetName("Ian");


			sock2.tickStream.BaseStream.Position = 0;
			netreader.ReadTick(new BinaryReader(sock2.tickStream.BaseStream));

			Assert.IsTrue(netreader.netObjects.Count == 1);
			Assert.AreEqual(p.GetName(), netreader.Get<NetClient.Player>(0).GetName());

		}
	}
}
