using System;
using System.IO;
using LNetwork;
using LNetwork.service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestLNetwork
{
	[TestClass]
	public class TestServerNetwork
	{
		[TestMethod]
		public void TestLoginOk()
		{
			Mock<DataSocket> mockDataSocket = new Mock<DataSocket>();

			mockDataSocket.Setup(m => m.handle());
			mockDataSocket.SetupSequence(m => m.getMessage()).Returns(
				PacketBuilder.New().Add<Int32>(PacketIds.PACKET_ID_CLIENT_LOGIN).Add("A").Add("B").Build()
				).Returns((byte[])null);

			Mock<ServerSocket> mockServerSocket = new Mock<ServerSocket>();
			mockServerSocket.Setup(m => m.listen(0));
			mockServerSocket.Setup(m => m.handle()).Returns(mockDataSocket.Object);


			Mock<ServerNetworkListener> mockServerNetworkListener = new Mock<ServerNetworkListener>();
			mockServerNetworkListener.Setup(m => m.OnConnected(It.IsAny<int>()));
			mockServerNetworkListener.Setup(m => m.OnLogin(It.IsAny<int>(), "A","B")).Returns(ServerNetworkLoginResponse.OK);


			Mock<ServerSocketBuilder> mockServerSocketBuilder = new Mock<ServerSocketBuilder>();
			mockServerSocketBuilder.Setup(m => m.Build()).Returns(mockServerSocket.Object);

			ServerNetwork serverNetwork = new ServerNetwork(mockServerSocketBuilder.Object, mockServerNetworkListener.Object);

			serverNetwork.Listen(0);
			for (int i = 0; i < 100; i++)
			{
				serverNetwork.Handle();
			}

			mockServerSocket.Verify(m => m.listen(0));

			mockServerNetworkListener.Verify(m => m.OnConnected(It.IsAny<int>()));
			mockServerNetworkListener.Verify(m => m.OnLogin(It.IsAny<int>(), "A", "B"));
		}

		[TestMethod]
		public void TestLoginFailure()
		{
			Mock<DataSocket> mockDataSocket = new Mock<DataSocket>();

			mockDataSocket.Setup(m => m.handle());
			mockDataSocket.SetupSequence(m => m.getMessage()).Returns(
				PacketBuilder.New().Add<Int32>(PacketIds.PACKET_ID_CLIENT_LOGIN).Add("A").Add("B").Build()
				).Returns((byte[])null);

			Mock<ServerSocket> mockServerSocket = new Mock<ServerSocket>();
			mockServerSocket.Setup(m => m.listen(0));
			mockServerSocket.Setup(m => m.handle()).Returns(mockDataSocket.Object);


			Mock<ServerNetworkListener> mockServerNetworkListener = new Mock<ServerNetworkListener>();
			mockServerNetworkListener.Setup(m => m.OnConnected(It.IsAny<int>()));
			mockServerNetworkListener.Setup(m => m.OnLogin(It.IsAny<int>(), "A", "B")).Returns(ServerNetworkLoginResponse.INCORRECT_USER);


			Mock<ServerSocketBuilder> mockServerSocketBuilder = new Mock<ServerSocketBuilder>();
			mockServerSocketBuilder.Setup(m => m.Build()).Returns(mockServerSocket.Object);

			ServerNetwork serverNetwork = new ServerNetwork(mockServerSocketBuilder.Object, mockServerNetworkListener.Object);

			serverNetwork.Listen(0);
			for (int i = 0; i < 100; i++)
			{
				serverNetwork.Handle();
			}

			mockServerSocket.Verify(m => m.listen(0));

			mockServerNetworkListener.Verify(m => m.OnConnected(It.IsAny<int>()));
			mockServerNetworkListener.Verify(m => m.OnLogin(It.IsAny<int>(), "A", "B"));
			mockServerNetworkListener.Verify(m => m.OnDisconnected(It.IsAny<int>()));
		}
	}
}
