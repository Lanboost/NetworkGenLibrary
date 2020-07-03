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

	/*
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
			if (master)
			{
				handler = network.GetServer(clientPacketGenerator, delegate () { return state; });
			}
			else
			{
				handler = network.GetClient(clientPacketGenerator, state);
			}

			state = new LockstepNetworkState(handler, clientPacketGenerator.Register(), clientPacketGenerator.Register(), master);


			action1 = state.RegisterLockstep(clientPacketGenerator.Register(), delegate (uint socketId, uint actionValue) { game.Action1(); return true; });
			action2 = state.RegisterLockstep(clientPacketGenerator.Register(), delegate (uint socketId, uint actionValue) { game.Action2(actionValue); return true; });
			tick = state.RegisterStepHandler(clientPacketGenerator.Register(), delegate (uint tick) { game.Tick(tick); });
			
		}


	}

	
	*/

	[TestClass]
	public class TestLockStep
	{

		[TestMethod]
		public void TestLockstepActionMaster()
		{
			uint SendPacketId = 0;
			uint TickPacketId =1;
			uint ActionPacketId =2;
			uint SendValue = 100;

			uint masterValue = 0;
			uint slaveValue = 0;


			Mock<INetworkSocketHandlerServer> mockClientHandler1 = new Mock<INetworkSocketHandlerServer>();
			Mock<INetworkSocketHandler> mockClientHandler2 = new Mock<INetworkSocketHandler>();

			LockstepNetworkState lockstep1 = new LockstepNetworkState(SendPacketId, TickPacketId, 10, true);
			lockstep1.Bind(mockClientHandler1.Object);
			var masterActionCall = lockstep1.RegisterLockstep<TempSend>(ActionPacketId, delegate(uint socketId, TempSend send)
			{
				masterValue = send.value;
				return true;
			});

			LockstepNetworkState lockstep2 = new LockstepNetworkState(SendPacketId, TickPacketId, 10, false);
			lockstep2.Bind(mockClientHandler2.Object);
			var slaveActionCall = lockstep2.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				slaveValue = send.value;
				return true;
			});

			mockClientHandler2.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep1.Handle(mockClientHandler1.Object, socketId, packetId, reader);
			});



			mockClientHandler1.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});

			mockClientHandler1.Setup(x => x.BroadCast(It.IsAny<byte[]>())).Callback(delegate (byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});





			masterActionCall(new TempSend(SendValue));

			Assert.AreEqual(SendValue, masterValue);
			Assert.AreEqual(SendValue, slaveValue);
			

		}


		[TestMethod]
		public void TestLockstepActionSlave()
		{
			uint SendPacketId = 0;
			uint TickPacketId = 1;
			uint ActionPacketId = 2;
			uint SendValue = 100;

			uint masterValue = 0;
			uint slaveValue = 0;


			Mock<INetworkSocketHandlerServer> mockClientHandler1 = new Mock<INetworkSocketHandlerServer>();
			Mock<INetworkSocketHandler> mockClientHandler2 = new Mock<INetworkSocketHandler>();

			LockstepNetworkState lockstep1 = new LockstepNetworkState(SendPacketId, TickPacketId, 10, true);
			lockstep1.Bind(mockClientHandler1.Object);
			var masterActionCall = lockstep1.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				masterValue = send.value;
				return true;
			});

			LockstepNetworkState lockstep2 = new LockstepNetworkState(SendPacketId, TickPacketId, 10, false);
			lockstep2.Bind(mockClientHandler2.Object);
			var slaveActionCall = lockstep2.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				slaveValue = send.value;
				return true;
			});

			mockClientHandler2.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep1.Handle(mockClientHandler1.Object, socketId, packetId, reader);
			});



			mockClientHandler1.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});

			mockClientHandler1.Setup(x => x.BroadCast(It.IsAny<byte[]>())).Callback(delegate (byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});





			slaveActionCall(new TempSend(SendValue));

			Assert.AreEqual(SendValue, masterValue);
			Assert.AreEqual(SendValue, slaveValue);
		}

		[TestMethod]
		public void TestLockstepUpdate()
		{
			uint SendPacketId = 0;
			uint TickPacketId = 1;
			uint ActionPacketId = 2;
			uint SendValue = 100;

			uint masterValue = 0;
			uint slaveValue = 0;

			uint masterTick = 0;
			uint slaveTick = 0;


			Mock<INetworkSocketHandlerServer> mockClientHandler1 = new Mock<INetworkSocketHandlerServer>();
			Mock<INetworkSocketHandler> mockClientHandler2 = new Mock<INetworkSocketHandler>();

			LockstepNetworkState lockstep1 = new LockstepNetworkState(SendPacketId, TickPacketId, 2, true);
			lockstep1.Bind(mockClientHandler1.Object);
			var masterActionCall = lockstep1.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				masterValue = send.value;
				return true;
			});

			var masterStep = lockstep1.RegisterStepHandler(delegate (uint stepId) { masterTick = stepId; });

			LockstepNetworkState lockstep2 = new LockstepNetworkState(SendPacketId, TickPacketId, 2, false);
			lockstep2.Bind(mockClientHandler2.Object);
			var slaveActionCall = lockstep2.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				slaveValue = send.value;
				return true;
			});

			var slaveStep = lockstep2.RegisterStepHandler(delegate (uint stepId) { slaveTick = stepId; });

			mockClientHandler2.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep1.Handle(mockClientHandler1.Object, socketId, packetId, reader);
			});



			mockClientHandler1.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});

			mockClientHandler1.Setup(x => x.BroadCast(It.IsAny<byte[]>())).Callback(delegate (byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});


			masterStep(0);
			slaveStep(10);

			Assert.AreEqual((uint)0, masterTick);
			Assert.AreEqual((uint)0, slaveTick);

			masterStep(10);

			Assert.AreEqual((uint)10, masterTick);
			Assert.AreEqual((uint)10, slaveTick);
		}

		[TestMethod]
		public void TestLockstepActionCorrectTickUpdate()
		{
			uint SendPacketId = 0;
			uint TickPacketId = 1;
			uint ActionPacketId = 2;
			uint SendValue = 100;

			uint masterValue = 0;
			uint slaveValue = 0;

			uint masterTick = 0;
			uint slaveTick = 0;


			Mock<INetworkSocketHandlerServer> mockClientHandler1 = new Mock<INetworkSocketHandlerServer>();
			Mock<INetworkSocketHandler> mockClientHandler2 = new Mock<INetworkSocketHandler>();

			LockstepNetworkState lockstep1 = new LockstepNetworkState(SendPacketId, TickPacketId, 10, true);
			lockstep1.Bind(mockClientHandler1.Object);
			var masterActionCall = lockstep1.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				masterValue = masterTick;
				return true;
			});

			var masterStep = lockstep1.RegisterStepHandler(delegate (uint stepId) { masterTick = stepId; });

			LockstepNetworkState lockstep2 = new LockstepNetworkState(SendPacketId, TickPacketId, 10, false);
			lockstep2.Bind(mockClientHandler2.Object);
			var slaveActionCall = lockstep2.RegisterLockstep<TempSend>(ActionPacketId, delegate (uint socketId, TempSend send)
			{
				slaveValue = slaveTick;
				return true;
			});

			var slaveStep = lockstep2.RegisterStepHandler(delegate (uint stepId) { slaveTick = stepId; });

			mockClientHandler2.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep1.Handle(mockClientHandler1.Object, socketId, packetId, reader);
			});



			mockClientHandler1.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});

			mockClientHandler1.Setup(x => x.BroadCast(It.IsAny<byte[]>())).Callback(delegate (byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				lockstep2.Handle(mockClientHandler2.Object, 0, packetId, reader);
			});


			masterStep(10);

			Assert.AreEqual((uint)10, masterTick);
			Assert.AreEqual((uint)10, slaveTick);

			slaveActionCall(new TempSend(10));

			Assert.AreEqual((uint)10, masterValue);
			Assert.AreEqual((uint)10, slaveValue);

			masterStep(11);

			masterActionCall(new TempSend(10));

			Assert.AreEqual((uint)11, masterValue);
			Assert.AreEqual((uint)11, slaveValue);
		}

	}
}
