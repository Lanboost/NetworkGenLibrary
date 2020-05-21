using LNetwork.normal;
using LNetwork.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public class ClientNetwork
	{
		private static int STATE_CONNECTED = 0;
		private static int STATE_AUTHENTICATED = 1;
		private static int STATE_ERROR = 2;

		ClientNetworkListener Listener;

		ClientSocket ClientSocket;
		DataSocket socket;
		int State;
		string Username;
		string Password;

		long lastPing;

		DataSocket CloseSocket;
		long CloseTimeout;

		public void Connect(string host, int port, string username, string password)
		{
			this.Username = username;
			this.Password = password;
			ClientSocket = new NetSocketClientSocket();
			ClientSocket.connect(host, port);
			
		}

		public void Handle()
		{
			if(CloseSocket != null)
			{
				CloseSocket.handle();
				if(CloseTimeout < CurrentMillis.Millis)
				{
					CloseSocket = null;
				}
			}

			if (socket != null)
			{
				socket.handle();

				while(true)
				{
					var msg = socket.getMessage();
					if(msg != null)
					{
						BinaryReader reader = new BinaryReader(new MemoryStream(msg));
						int cmd = reader.ReadInt32();
						if(cmd == PacketIds.PACKET_ID_SERVER_CLOSE)
						{
							State = STATE_ERROR;
							var reason = reader.ReadString();
							Listener.OnError(reason);

							var mem = new MemoryStream();
							BinaryWriter writer = new BinaryWriter(mem);
							writer.Write(PacketIds.PACKET_ID_CLIENT_ACCEPT_CLOSE);
							writer.Flush();
							Send(mem.ToArray());
							CloseTimeout = CurrentMillis.Millis + 10 * 1000;
							CloseSocket = socket;
						}

						if (State == STATE_CONNECTED)
						{
							if(cmd == PacketIds.PACKET_ID_SERVER_AUTH)
							{
								State = STATE_AUTHENTICATED;
								Listener.OnAuthenticated();
							}
						}
						else if(State == STATE_AUTHENTICATED)
						{
							Listener.OnMessage(msg);
						}
					}
					else
					{
						break;
					}
				}

				if(lastPing+20*1000 < CurrentMillis.Millis)
				{
					var mem = new MemoryStream();
					BinaryWriter writer = new BinaryWriter(mem);
					writer.Write(PacketIds.PACKET_ID_CLIENT_PING);
					writer.Flush();
					Send(mem.ToArray());
				}
			}
			else
			{
				if (ClientSocket != null)
				{
					socket = ClientSocket.handle();
					State = STATE_CONNECTED;
					Listener.OnConnected();

					var mem = new MemoryStream();
					BinaryWriter writer = new BinaryWriter(mem);
					writer.Write(PacketIds.PACKET_ID_CLIENT_LOGIN);
					writer.Write(Username);
					writer.Write(Password);
					Username = null;
					Password = null;
					writer.Flush();
					Send(mem.ToArray());
				}
			}
		}

		public void Send(byte[] msg)
		{
			socket.send(msg);
		}

	}
}
