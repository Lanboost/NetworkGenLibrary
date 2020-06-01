using LNetwork;
using LNetwork.lockstep;
using LNetwork.test;
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

	public class LockstepGame
	{

		public INetworkSocketHandler handler;

		public TestGame game = new TestGame();

		public StandardNetworkPacketIdGenerator clientPacketGenerator = new StandardNetworkPacketIdGenerator();
		public LockstepNetworkState state;

		public Func<uint, uint> action1;
		public Func<uint, uint> action2;

		public Action<uint> tick;

		public LockstepGame(AutoConnectedNetwork network, bool master)
		{
			state = new LockstepNetworkState(clientPacketGenerator, master);

			if (master)
			{
				handler = network.GetServer(clientPacketGenerator, delegate () { return state; });
			}
			else
			{
				handler = network.GetClient(clientPacketGenerator, state);
			}

			action1 = state.RegisterLockstep(handler, clientPacketGenerator.Register(), delegate (uint socketId, uint actionValue) { game.Action1(); return true; });
			action2 = state.RegisterLockstep(handler, clientPacketGenerator.Register(), delegate (uint socketId, uint actionValue) { game.Action2(actionValue); return true; });
			tick = state.RegisterStepHandler(handler, clientPacketGenerator.Register(), delegate (uint tick) { game.Tick(tick); });
			
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
			


			for (uint i=0; i<=10; i++)
			{
				lockstep1.tick(i);
			}

			Assert.AreEqual((UInt32) 10, lockstep1.game.ticks);
			Assert.AreEqual((UInt32) 10, lockstep2.game.ticks);
			Assert.AreEqual((UInt32)10, lockstep3.game.ticks);


			lockstep1.tick(100);
			lockstep1.action1(0);

			Assert.AreEqual((UInt32)100, lockstep1.game.action1);
			Assert.AreEqual((UInt32)100, lockstep2.game.action1);
			Assert.AreEqual((UInt32)100, lockstep3.game.action1);

		}

	}
}
