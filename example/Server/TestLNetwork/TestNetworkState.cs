using LNetwork;
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

	[Serializable]
	class TempSend
	{
		public uint value;

		public TempSend(uint value)
		{
			this.value = value;
		}
	}

	[Serializable]
	class TempReceive
	{
		public uint value;

		public TempReceive(uint value)
		{
			this.value = value;
		}
	}


	[TestClass]
	public class TestNetworkState
	{
		[TestMethod]
		public void TestRPC()
		{
			uint SendPacketId = 0;
			uint SendValue = 100;
			uint ReceiveValue = 101;

			StandardRPCNetworkSocketState rpc1 = new StandardRPCNetworkSocketState();
			var clientCall = rpc1.RegisterRPC<TempSend, TempReceive>(SendPacketId);

			StandardRPCNetworkSocketState rpc2 = new StandardRPCNetworkSocketState();
			rpc2.RegisterRPCHandler<TempSend, TempReceive>(SendPacketId, delegate(uint socketId, uint packetId, TempSend ind)
			{

				Assert.AreEqual(SendValue, ind.value);

				TempReceive outd = new TempReceive(ReceiveValue);

				return outd;
			});

			Mock<INetworkSocketHandler> mockClientHandler = new Mock<INetworkSocketHandler>();
			mockClientHandler.Setup(x => x.Send(It.IsAny<UInt32>(), It.IsAny<byte[]>())).Callback(delegate (uint socketId, byte[] message)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(message));
				uint packetId = reader.ReadUInt32();
				rpc2.Handle(mockClientHandler.Object, socketId, packetId, reader);
			});

			var wasCalled = false;

			clientCall.Invoke(mockClientHandler.Object, SendPacketId, new TempSend(SendValue), delegate (TempReceive receive)
			{
				wasCalled = true;
				Assert.AreEqual(ReceiveValue, receive.value);
			});

			Assert.IsTrue(wasCalled);

		}
	}
}
