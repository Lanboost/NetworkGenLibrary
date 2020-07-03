using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LNetwork.websocket
{
    class WebSocketDataSocket : WebSocketBehavior, IDataSocket
    {
        bool connected = false;
        bool error = false;

        byte[] lenbuffer = new byte[2];
        byte[] messagebuffer;
        int messagelen = -1;
        int read = 0;
        int msgtype = 0;

        List<byte[]> messages = new List<byte[]>();

        object olock = new object();


        protected override void OnOpen()
        {
            lock (olock)
            {
                connected = true;
            }
            //add to serversocket....
            WebSocketServerSocket server = WebSocketServerSocket.getInstance();
            lock(server.olock)
            {
                server.newsockets.Add(this);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            lock (olock)
            {
                connected = false;
            }
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            lock (olock)
            {
                error = true;
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            
            readResult(e.RawData);
        }



        public void readResult(byte[] data)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(data));
            int left = data.Length;
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
                        br.Read(lenbuffer, 0, left);
                        msgtype = 1;
                        break;
                    }

                }
                //we have read 1 byte of next packet len (FeelsBadMan)
                else if (msgtype == 1)
                {
                    br.Read(lenbuffer, 1, 1);
                    left -= 1;
                    msgtype = 2;
                    read = 0;
                    BinaryReader brtemp = new BinaryReader(new MemoryStream(lenbuffer));
                    messagelen = brtemp.ReadUInt16();
                    messagebuffer = new byte[messagelen];
                }
                //We have read packet len and is waiting for rest of the data
                else if (msgtype == 2)
                {
                    int toread = this.messagelen - read;
                    if (left >= toread)
                    {
                        br.Read(messagebuffer, read, toread);
                        lock (olock)
                        {
                            messages.Add(messagebuffer);
                        }
                        left -= toread;
                        msgtype = 0;
                        if (left == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        br.Read(messagebuffer, read, toread);
                        read += toread;
                        left -= toread;
                        break;
                    }
                }

            }
        }


        public byte[] getMessage()
        {
            byte[] m = null;
            lock (olock)
            {
                if (this.messages.Count > 0)
                {
                    m = this.messages[0];
                    messages.RemoveAt(0);

                }
            }
            return m;
        }

        public void handle()
        {

        }

        public bool isConnected()
        {
            bool v = false;
            lock (olock)
            {
                v = connected;
            }
            return v;
        }

        public bool isError()
        {
            bool v = false;
            lock (olock)
            {
                v = error;
            }
            return v;
        }

        public void send(byte[] message)
        {
            byte[] badbuff = new byte[message.Length + 2];
            message.CopyTo(badbuff, 2);
            BinaryWriter bw = new BinaryWriter(new MemoryStream(badbuff));
            bw.Write((UInt16)message.Length);
            bw.Close();

            Send(badbuff);
        }

		public string ip()
		{
			throw new NotImplementedException();
		}

		public void setError()
		{
			throw new NotImplementedException();
		}

		public void close()
		{
			throw new NotImplementedException();
		}
	}
}
