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
	public class TestNetworks
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

			ClientNetwork clientNetwork = new ClientNetwork(mockClientSocketBuilder.Object);

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


			ServerNetwork serverNetwork = new ServerNetwork(
				mockSocketBuilder.Object, 
				delegate(uint socketId, DataSocket socket, NetworkSocketStateRouter rotuer)
			{

			});

			serverNetwork.Listen(port);

			for (int i = 0; i < 100; i++)
			{
				serverNetwork.Handle();
			}

			var first = serverNetwork.GetSockets().First();

			serverNetwork.Send(first.Item1, toSend);
			serverNetwork.BroadCast(toSend);
			

			mockSocket.Verify(x => x.listen(port));
			mockDataSocket.Verify(m => m.send(toSend), Times.Exactly(2));
		}
	}
}
