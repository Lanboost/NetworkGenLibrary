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

			Mock<IDataSocket> mockDataSocket = new Mock<IDataSocket>();

			mockDataSocket.Setup(x => x.getMessage()).Returns<byte[]>(null);

			Mock<IClientSocket> mockClientSocket = new Mock<IClientSocket>();

			mockClientSocket.Setup(x => x.connect(It.IsAny<string>(), It.IsAny<Int32>()));
			mockClientSocket.Setup(x => x.handle()).Returns(mockDataSocket.Object);

			Mock<IBuilder<IClientSocket>> mockClientSocketBuilder = new Mock<IBuilder<IClientSocket>>();
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

			Mock<IDataSocket> mockDataSocket = new Mock<IDataSocket>();

			mockDataSocket.Setup(x => x.getMessage()).Returns<byte[]>(null);

			Mock<IServerSocket> mockSocket = new Mock<IServerSocket>();
			
			mockSocket.SetupSequence(x => x.handle()).Returns((IDataSocket)null).Returns(mockDataSocket.Object).Returns((IDataSocket)null);

			Mock<IBuilder<IServerSocket>> mockSocketBuilder = new Mock<IBuilder<IServerSocket>>();
			mockSocketBuilder.Setup(x => x.Build()).Returns(mockSocket.Object);


			ServerNetwork serverNetwork = new ServerNetwork(
				mockSocketBuilder.Object, 
				delegate(uint socketId, IDataSocket socket, NetworkSocketStateRouter rotuer)
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
