using System;
using System.IO;
using System.Linq;
using LNetwork;
using LNetwork.service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestLNetwork
{
	[TestClass]
	public class TestrNetworks
	{
		[TestMethod]
		public void TestClientNetwork()
		{
			byte[] toSend = new byte[10];
			string host = "asd";
			int port = 1000;

			Mock<DataSocket> mockDataSocket = new Mock<DataSocket>();

			mockDataSocket.Setup(x => x.getMessage()).Returns<byte[]>(null);

			Mock<ClientSocket> mockClientSocket = new Mock<ClientSocket>();

			mockClientSocket.Setup(x => x.connect(It.IsAny<string>(), It.IsAny<Int32>()));
			mockClientSocket.Setup(x => x.handle()).Returns(mockDataSocket.Object);

			Mock<IBuilder<ClientSocket>> mockClientSocketBuilder = new Mock<IBuilder<ClientSocket>>();
			mockClientSocketBuilder.Setup(x => x.Build()).Returns(mockClientSocket.Object);


			StandardNetworkPacketIdGenerator clientPacketGenerator = new StandardNetworkPacketIdGenerator();
			ClientNetwork clientNetwork = new ClientNetwork(mockClientSocketBuilder.Object, clientPacketGenerator);

			clientNetwork.Connect(host, port);

			for (int i = 0; i < 100; i++)
			{
				clientNetwork.Handle();
			}

			clientNetwork.Send(toSend);



			mockClientSocket.Verify(x => x.connect(host, port));
			mockDataSocket.Verify(m => m.send(toSend));
		}

		[TestMethod]
		public void TestServerNetwork()
		{
			byte[] toSend = new byte[10];
			int port = 1000;

			Mock<DataSocket> mockDataSocket = new Mock<DataSocket>();

			mockDataSocket.Setup(x => x.getMessage()).Returns<byte[]>(null);

			Mock<ServerSocket> mockSocket = new Mock<ServerSocket>();
			
			mockSocket.SetupSequence(x => x.handle()).Returns((DataSocket)null).Returns(mockDataSocket.Object).Returns((DataSocket)null);

			Mock<IBuilder<ServerSocket>> mockSocketBuilder = new Mock<IBuilder<ServerSocket>>();
			mockSocketBuilder.Setup(x => x.Build()).Returns(mockSocket.Object);


			Mock<NetworkSocketState> mockNetworkSocketState = new Mock<NetworkSocketState>();
			Mock<IBuilder<NetworkSocketState>> mockNetworkSocketStateBuilder = new Mock<IBuilder<NetworkSocketState>>();
			mockNetworkSocketStateBuilder.Setup(x => x.Build()).Returns(mockNetworkSocketState.Object);


			StandardNetworkPacketIdGenerator packetGenerator = new StandardNetworkPacketIdGenerator();
			ServerNetwork serverNetwork = new ServerNetwork(packetGenerator, mockSocketBuilder.Object, mockNetworkSocketStateBuilder.Object);

			serverNetwork.Listen(port);

			for (int i = 0; i < 100; i++)
			{
				serverNetwork.Handle();
			}

			var first = serverNetwork.GetSockets().First();

			serverNetwork.Send(first.Item1, toSend);
			serverNetwork.BroadCast(toSend);

			Assert.AreEqual(mockNetworkSocketState.Object, serverNetwork.GetSocketState(first.Item1));

			mockSocket.Verify(x => x.listen(port));
			mockDataSocket.Verify(m => m.send(toSend), Times.Exactly(2));
		}
	}
}
