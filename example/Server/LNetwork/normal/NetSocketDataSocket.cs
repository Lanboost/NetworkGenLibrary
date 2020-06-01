using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LNetwork.normal
{
	public class NetSocketDataSocket : DataSocket
    {

        static readonly object _object = new object();

        Socket socket;
        SocketAsyncEventArgs asyncevent = new SocketAsyncEventArgs();

        bool connected;
        int state = 1;
        bool error;
        string errormessage;

        byte[] buffer = new byte[1000];


        byte[] lenbuffer = new byte[2];
        byte[] messagebuffer;
        int messagelen = -1;
        int read = 0;
        int msgtype = 0;

        public NetSocketDataSocket(Socket socket)
        {
            this.socket = socket;
            asyncevent.Completed += readResult;
        }

        public override void close()
        {
            socket.Close();
        }

        List<byte[]> messages = new List<byte[]>();
        List<byte[]> readmessages = new List<byte[]>();

        public override bool isConnected()
        {
            return state != 0;
        }

        public override bool isError()
        {
            return error;
        }

		public override void send(byte[] message)
		{
			SocketAsyncEventArgs asynceventsend = new SocketAsyncEventArgs();
			byte[] badbuff = new byte[message.Length + 2];
			message.CopyTo(badbuff, 2);
			BinaryWriter bw = new BinaryWriter(new MemoryStream(badbuff));
			bw.Write((UInt16)message.Length);
			bw.Close();

			asynceventsend.Completed += checkSend;

			asynceventsend.SetBuffer(badbuff, 0, badbuff.Length);
			if (!socket.SendAsync(asynceventsend))
			{
				checkSend(null, asynceventsend);
			}
		}

		public void checkSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                this.error = true;
                //Console.WriteLine("Who knows, something didn't work");
            }
            else
            {
                //Console.WriteLine("Sent ok!");
            }
        }

        public void readResult(object sender, SocketAsyncEventArgs e)
        {
            lock (_object)
            {
                if (asyncevent.SocketError == SocketError.Success)
                {
                    //sets that we have read
                    state = 1;

                    byte[] data = asyncevent.Buffer;
                    BinaryReader br = new BinaryReader(new MemoryStream(data));


                    int left = asyncevent.BytesTransferred;
                    while (true)
                    {
                        //we have read nothing
                        if (msgtype == 0)
                        {
                            //we can read len
                            if (left >= 2)
                            {
                                messagelen = br.ReadUInt16();
                                messagebuffer = new byte[messagelen];
                                left -= 2;
                                msgtype = 2;
                                read = 0;
                            }
                            else
                            {
								if (left > 0)
								{
									br.Read(lenbuffer, 0, left);
									msgtype = 1;
								}
                                break;
                            }

                        }
                        //we have read 1 byte of next packet len (FeelsBadMan)
                        else if (msgtype == 1)
                        {
							if (left >= 1)
							{
								br.Read(lenbuffer, 1, 1);
								left -= 1;
								msgtype = 2;
								read = 0;
								BinaryReader brtemp = new BinaryReader(new MemoryStream(lenbuffer));
								messagelen = brtemp.ReadUInt16();
								messagebuffer = new byte[messagelen];
							}
							else
							{
								break;
							}
                        }
                        //We have read packet len and is waiting for rest of the data
                        else if (msgtype == 2)
                        {
                            int toread = this.messagelen - read;
                            if (left >= toread)
                            {
                                br.Read(messagebuffer, read, toread);
                                messages.Add(messagebuffer);
                                left -= toread;
                                msgtype = 0;
                                if (left == 0)
                                {
                                    break;
                                }
                            }
                            else
                            {
								if (left > 0)
								{
									br.Read(messagebuffer, read, left);
									read += left;
									left -= left;
								}
								else
								{
									break;
								}
                            }
                        }

                    }
                }
                else
                {
                    state = 0;
                    error = true;
                    errormessage = "Read error.";
                }
            }
        }

        public override void handle()
        {
            lock (_object)
            {
                //trying to read data
                if (state == 1)
                {
                    state = 2;
                    asyncevent.SetBuffer(buffer, 0, 1000);

                    if (!socket.ReceiveAsync(asyncevent))
                    {
                        readResult(null, null);
                    }
                }
                //waiting for data read
                else if (state == 2)
                {

                }
            }
        }

        public override byte[] getMessage()
        {
            lock (_object)
            {
                if (this.messages.Count > 0)
                {
                    byte[] m = this.messages[0];
                    messages.RemoveAt(0);
                    readmessages.Add(m);
                    return m;
                }
            }
            return null;
        }

        public override string ip()
        {
            try
            {
                IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                return "" + remoteIpEndPoint.Address;
            }
            catch
            {

            }

            return "";
        }

        public override void setError()
        {
            this.error = true;
        }
    }
}
