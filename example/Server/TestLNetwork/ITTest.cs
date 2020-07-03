using LNetwork;
using LNetwork.lockstep;
using LNetwork.plugins.auth;
using LNetwork.service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLNetwork
{
	[TestClass]
	public class ITTest
	{

		[TestMethod]
		public void TestClientServerConnect()
		{
			bool serverOk = false;
			var serverSocket = new ServerNetwork(
				new BuilderWrapper<IServerSocket>(
					delegate ()
					{
						return new LNetwork.normal.NetSocketServerSocket();
					}
				),
				delegate (uint socketId, IDataSocket socket, NetworkSocketStateRouter router)
				{
					serverOk = true;
				}
			);

			serverSocket.Listen(8000);

			var client = new ClientNetwork(
				new BuilderWrapper<IClientSocket>(
					delegate ()
					{
						return new LNetwork.normal.NetSocketClientSocket();
					}
				)
			);

			client.Connect("127.0.0.1", 8000);


			for(int i=0; i<100000; i++)
			{
				serverSocket.Handle();
				client.Handle();
			}

			Assert.IsTrue(serverOk);
			Assert.IsNotNull(client.GetSocket());
		}


		[TestMethod]
		public void TestClientServerAuth()
		{
			string username = "test";
			string password = "123";

			bool clientLoginOk = false;

			uint masterStepId = 0;
			uint slaveStepId = 0;

			ServerNetwork serverSocket = null;
			AuthenticationServerPlugin authServer = null;

			LockstepNetworkState lockstepServer = null;

			authServer = new AuthenticationServerPlugin(
				0,
				delegate (uint socketId, uint packetId, LoginPacket login) {
					Assert.AreEqual(username, login.username);
					Assert.AreEqual(password, login.password);

					serverSocket.GetSocketRouter(socketId).Detach(authServer);
					serverSocket.GetSocketRouter(socketId).Attach(lockstepServer);

					return new LoginResponsePacket(true);
				}
			);

			lockstepServer = new LockstepNetworkState(
				0, 1, 2, true
			);

			var serverStep = lockstepServer.RegisterStepHandler(delegate (uint stepId) { masterStepId = stepId; });

			serverSocket = new ServerNetwork(
				new BuilderWrapper<IServerSocket>(
					delegate ()
					{
						return new LNetwork.normal.NetSocketServerSocket();
					}
				),
				delegate (uint socketId, IDataSocket socket, NetworkSocketStateRouter router)
				{
					router.Attach(authServer);
				}
			);

			authServer.Bind(serverSocket);
			lockstepServer.Bind(serverSocket);
			serverSocket.Listen(8001);

			LockstepNetworkState lockstepClient = null;

			lockstepClient = new LockstepNetworkState(
				0, 1, 2, false
			);

			lockstepClient.RegisterStepHandler(delegate (uint stepId) { slaveStepId = stepId; });

			ClientNetwork client = null;
			AuthenticationClientPlugin authClient = null;

			authClient = new AuthenticationClientPlugin(
				0,
				delegate (LoginResponsePacket response) {
					clientLoginOk = true;
					Assert.IsTrue(response.success);

					client.GetSocketRouter().Detach(authClient);
					client.GetSocketRouter().Attach(lockstepClient);
				}
			);

			client = new ClientNetwork(
				new BuilderWrapper<IClientSocket>(
					delegate ()
					{
						return new LNetwork.normal.NetSocketClientSocket();
					}
				)
			);

			authClient.Bind(client);
			lockstepClient.Bind(client);
			client.Connect("127.0.0.1", 8001);


			for (int i = 0; i < 100000; i++)
			{
				serverSocket.Handle();
				client.Handle();
			}


			client.GetSocketRouter().Attach(authClient);
			authClient.Login(username, password);

			for (int i = 0; i < 100000; i++)
			{
				serverSocket.Handle();
				client.Handle();
			}

			Assert.IsTrue(clientLoginOk);

			serverStep(10);

			for (int i = 0; i < 100000; i++)
			{
				serverSocket.Handle();
				client.Handle();
			}

			Assert.AreEqual((uint)10, masterStepId);
			Assert.AreEqual((uint)10, slaveStepId);

			//Assert.IsNotNull(client.GetSocket());
		}
	}
}
