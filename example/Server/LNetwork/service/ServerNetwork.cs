using LNetwork.normal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.service
{
	public enum ServerNetworkLoginResponse
	{
		OK,
		INCORRECT_USER,
		INCORRECT_PASSWORD
	}

	class ConnectionData
	{
		public DataSocket DataSocket { get; set; }
		public int State { get; set; }
		public long Timeout { get; set; }
	}

	public class ServerNetwork
	{

		private static int STATE_CONNECTED = 0;
		private static int STATE_AUTHENTICATED = 1;
		private static int STATE_ERROR = 2;

		Dictionary<int, ConnectionData> ConnectedSockets = new Dictionary<int, ConnectionData>();
		ServerSocketBuilder ServerSocketBuilder;
		ServerSocket ServerSocket;
		ServerNetworkListener Listener;

		public ServerNetwork(ServerSocketBuilder serverSocketBuilder, ServerNetworkListener listener)
		{
			ServerSocketBuilder = serverSocketBuilder;
			Listener = listener;
		}

		int cidCounter = 0;

		public int GetConnectedCount()
		{
			return this.ConnectedSockets.Count;
		}

		private int GetNextCID()
		{
			return cidCounter++;
		}

		private long GetCloseTimeout()
		{
			return CurrentMillis.Millis+10*1000;
		}

		public void Listen(int port)
		{
			if(ServerSocket != null)
			{
				throw new Exception("Socket already running!");
			}

			ServerSocket = ServerSocketBuilder.Build();
			ServerSocket.listen(port);
		}

		public void Handle()
		{
			long currentMillis = CurrentMillis.Millis;
			long timeoutTimestamp = currentMillis+30*1000;

			var newSocket = ServerSocket.handle();
			if(newSocket != null)
			{
				ConnectionData connectionData = new ConnectionData();
				connectionData.DataSocket = newSocket;
				connectionData.State = STATE_CONNECTED;
				connectionData.Timeout = timeoutTimestamp;
				var cid = GetNextCID();
				ConnectedSockets.Add(cid, connectionData);

				Listener.OnConnected(cid);
			}

			List<int> timed = new List<int>();
			foreach (var pair in ConnectedSockets)
			{
				pair.Value.DataSocket.handle();
				while (true)
				{
					var msg = pair.Value.DataSocket.getMessage();

					if (msg != null)
					{
						BinaryReader reader = new BinaryReader(new MemoryStream(msg));
						int cmd = reader.ReadInt32();

						if(cmd == PacketIds.PACKET_ID_CLIENT_PING)
						{
							if (pair.Value.State != STATE_AUTHENTICATED)
							{
								pair.Value.Timeout = timeoutTimestamp;
							}
						}


						if (pair.Value.State == STATE_CONNECTED)
						{
							if (cmd == PacketIds.PACKET_ID_CLIENT_LOGIN)
							{
								var username = reader.ReadString();
								var password = reader.ReadString();
								var ret = Listener.OnLogin(pair.Key, username, password);

								if (ret == ServerNetworkLoginResponse.OK)
								{
									pair.Value.State = STATE_AUTHENTICATED;

									var mem = new MemoryStream();
									BinaryWriter writer = new BinaryWriter(mem);
									writer.Write(PacketIds.PACKET_ID_SERVER_AUTH);
									writer.Flush();
									Send(pair.Key, mem.ToArray());

								}
								else
								{
									if (ret == ServerNetworkLoginResponse.INCORRECT_USER)
									{
										CloseSocket(pair.Key, "No such user");
									}
									else if (ret == ServerNetworkLoginResponse.INCORRECT_PASSWORD)
									{
										CloseSocket(pair.Key, "Incorrect password");
									}
								}
							}
							else
							{
								CloseSocket(pair.Key, "Network error");
							}
						}
						else if (pair.Value.State == STATE_AUTHENTICATED)
						{
							Listener.OnMessage(pair.Key, msg);
						}
						else if (pair.Value.State == STATE_ERROR)
						{
							if (cmd == PacketIds.PACKET_ID_CLIENT_ACCEPT_CLOSE)
							{
								pair.Value.DataSocket.close();
								pair.Value.Timeout = 0;
							}
						}
					}
					else
					{
						break;
					}
				}

				if(pair.Value.Timeout < currentMillis)
				{
					timed.Add(pair.Key);
				}
			}

			foreach(var cid in timed)
			{
				if(ConnectedSockets[cid].State == STATE_ERROR)
				{
					ConnectedSockets.Remove(cid);
				}
				else
				{
					CloseSocket(cid, "Socket timed out");
				}
			}
		}

		public void CloseSocket(int cid, string reason = null)
		{
			if(reason != null)
			{
				Listener.OnError(cid);
			}
			Listener.OnDisconnected(cid);

			ConnectedSockets[cid].State = STATE_ERROR;
			SendClose(cid, reason);
			ConnectedSockets[cid].Timeout = GetCloseTimeout();
		}

		private void SendClose(int cid, string reason)
		{
			var mem = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(mem);
			writer.Write(PacketIds.PACKET_ID_SERVER_CLOSE);
			writer.Write(reason);
			writer.Flush();
			Send(cid, mem.ToArray(), true);
		}

		public void Send(int cid, byte[] data, bool ignoreError = false)
		{
			try
			{
				ConnectedSockets[cid].DataSocket.send(data);
			}
			catch(Exception e) {
				if(!ignoreError)
				{
					CloseSocket(cid, "Network Error");
				}
			}
		}
	}
}
