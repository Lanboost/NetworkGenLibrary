using LNetwork;
using LNetwork.lockstep;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLNetwork
{
	public class TestGame
	{
		public uint action1 = 0;
		public uint action2 = 0;
		public uint ticks = 0;

		public void Tick(uint tick)
		{
			ticks = tick;
		}

		public void Action1()
		{
			action1 = ticks;
		}

		public void Action2(uint actionValue)
		{
			action2 = actionValue;
		}

	}

	public class AutoConnectedNetwork : IBuilder<INetworkSocketHandler>
	{
		Dictionary<uint, TestNetworkSocketHandler> network = new Dictionary<uint, TestNetworkSocketHandler>();
		UIDCounter UIDCounter = new UIDCounter();

		public INetworkSocketHandler Build()
		{
			var t = new TestNetworkSocketHandler();
			var socketId = UIDCounter.Get();

			

			foreach (var n in network)
			{
				n.Value.AddConnection(t, socketId);

				t.AddConnection(n.Value, n.Key);
			}

			return t;
		}
	}

	public class LockstepGame
	{

		public INetworkSocketHandler handler;

		public TestGame game = new TestGame();

		public StandardNetworkPacketIdGenerator clientPacketGenerator = new StandardNetworkPacketIdGenerator();
		public LockstepNetworkState state;

		public Func<uint, uint> action1;
		public Func<uint, uint> action2;

		public LockstepGame(AutoConnectedNetwork network, bool master)
		{
			handler = network.Build();


			state = new LockstepNetworkState(clientPacketGenerator, delegate (uint tick) { game.Tick(tick); }, master);


			action1 = state.RegisterLockstep(handler, clientPacketGenerator.Register(), delegate (uint socketId, uint actionValue) { game.Action1(); return true; });
			action2 = state.RegisterLockstep(handler, clientPacketGenerator.Register(), delegate (uint socketId, uint actionValue) { game.Action2(actionValue); return true; });
		}


	}


	public class TestNetworkSocketHandler : INetworkSocketHandler
	{
		Dictionary<TestNetworkSocketHandler, uint> connections = new Dictionary<TestNetworkSocketHandler, uint>();
		Dictionary<uint, NetworkSocketState> states = new Dictionary<uint, NetworkSocketState>();
		Dictionary<uint, INetworkSocketHandler> sockets = new Dictionary<uint, INetworkSocketHandler>();
		List<Tuple<uint, byte[]>> messages = new List<Tuple<uint, byte[]>>();

		Random r = new Random();
		int sleepCount = 0;

		bool IsSleeping()
		{
			int now = sleepCount--;
			if(now < 0)
			{
				sleepCount = r.Next(0, 100);
				return false;
			}
			return true;
		}

		public void AddConnection(TestNetworkSocketHandler connection, uint socketId)
		{
			connections.Add(connection, socketId);
		}


		public void AddMessage(TestNetworkSocketHandler connection, byte[] message)
		{
			messages.Add(Tuple.Create(connections[connection], message));
		}


		public void BroadCast(byte[] msg)
		{
			foreach(var conn in connections)
			{
				conn.Key.AddMessage(this, msg);
			}
		}

		public DataSocket GetSocket(uint socketId)
		{
			throw new NotImplementedException();
		}

		public int GetSocketCount()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Tuple<uint, DataSocket>> GetSockets()
		{
			throw new NotImplementedException();
		}

		public NetworkSocketState GetSocketState(uint socketId)
		{
			return states[socketId];
		}

		public void Handle()
		{
			if(!IsSleeping())
			{
				if (messages.Count > 0)
				{
					var data = messages[0];
					var socketId = data.Item1;
					var msg = data.Item2;

					messages.RemoveAt(0);

					BinaryReader reader = new BinaryReader(new MemoryStream(msg));
					uint packetId = reader.ReadUInt32();

					states[socketId].Handle(this, socketId, packetId, reader);
				}
			}
		}

		public void Send(uint socketId, byte[] msg)
		{
			foreach(var conn in connections)
			{
				if(conn.Value == socketId)
				{
					conn.Key.AddMessage(this, msg);
				}
			}
		}

		public void Send(byte[] msg)
		{
			if (connections.Count != 0)
			{
				throw new Exception("To use connection count must be 1");
			}
			var c = connections.First();
			c.Key.AddMessage(this, msg);
		}

		public void SetSocketState(uint socketId, NetworkSocketState networkSocketState)
		{
			states[socketId] = networkSocketState;
		}
	}

	[TestClass]
	public class TestLockStep
	{

		[TestMethod]
		public void TestLockStepLocking()
		{
			var network = new AutoConnectedNetwork();

			var lockstep1 = new LockstepGame(network, true);
			var lockstep2 = new LockstepGame(network, false);
			var lockstep3 = new LockstepGame(network, false);


			Action ensureTicks = delegate ()
			{
				for (int i = 0; i < 1000; i++)
				{
					lockstep1.handler.Handle();
					lockstep1.handler.Handle();
					lockstep1.handler.Handle();
				}
			};


			for (uint i=0; i<=10; i++)
			{
				lockstep1.state.StepLockstep(lockstep1.handler, i);
			}

			ensureTicks();

			Assert.AreEqual((UInt32) 10, lockstep1.game.ticks);
			Assert.AreEqual((UInt32) 10, lockstep2.game.ticks);
			Assert.AreEqual((UInt32)10, lockstep3.game.ticks);


			lockstep1.state.StepLockstep(lockstep1.handler, 100);
			lockstep1.action1(0);

			ensureTicks();

			Assert.AreEqual((UInt32)100, lockstep1.game.action1);
			Assert.AreEqual((UInt32)100, lockstep2.game.action1);
			Assert.AreEqual((UInt32)100, lockstep3.game.action1);

		}

	}
}
