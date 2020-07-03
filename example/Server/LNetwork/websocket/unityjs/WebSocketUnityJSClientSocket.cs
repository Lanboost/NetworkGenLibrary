using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace LNetwork.websocket.unityjs
{
    public class WebSocketUnityJSClientSocket: IDataSocket, IClientSocket
    {

        byte[] readbuff = new byte[1000];

        byte[] lenbuffer = new byte[2];
        byte[] messagebuffer;
        int messagelen = -1;
        int read = 0;
        int msgtype = 0;
        List<byte[]> messages = new List<byte[]>();

        [DllImport("__Internal")]
        private static extern void jsconnect(string str);

        [DllImport("__Internal")]
        private static extern bool jsisConnected();

        [DllImport("__Internal")]
        private static extern bool jsisError();

        [DllImport("__Internal")]
        private static extern int jspollLength();

        [DllImport("__Internal")]
        private static extern int jspollData(byte[] data, int max);

        [DllImport("__Internal")]
        private static extern void jssendData(byte[] data, int length);

        void IClientSocket.connect(string server, int port)
        {
            jsconnect("ws://"+server+":"+port+"/websocket");
        }

        public byte[] getMessage()
        {
            if (this.messages.Count > 0)
            {
                byte[] m = this.messages[0];
                messages.RemoveAt(0);
                return m;
            }
            return null;
        }

        IDataSocket IClientSocket.handle()
        {
            if(jsisConnected())
            {
                return this;
            }
            return null;
        }

        public void close()
        {
            //socket.Close();
        }

        public void send(byte[] message)
        {
            byte[] badbuff = new byte[message.Length + 2];
            message.CopyTo(badbuff, 2);
            BinaryWriter bw = new BinaryWriter(new MemoryStream(badbuff));
            bw.Write((UInt16)message.Length);
            bw.Close();

            jssendData(badbuff, badbuff.Length);
        }


        public void readResult(byte[] data, int len)
        {
            
            BinaryReader br = new BinaryReader(new MemoryStream(data));


            int left = len;
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
                        br.Read(messagebuffer, read, left);
                        read += left;
                        left -= left;
                        break;
                    }
                }

            }
        }

        void IDataSocket.handle()
        {
            int len = jspollLength();
            if(len > 0)
            {
                int read = jspollData(readbuff, len);
                if (read > 0)
                {
                    readResult(readbuff, read);
                }
                else
                {
                    throw new Exception("Buffer not large enuff");
                }
            }
        }

        bool IDataSocket.isConnected()
        {
            return jsisConnected();
        }

        bool IDataSocket.isError()
        {
            return jsisError();
        }

        bool IClientSocket.isError()
        {
            return jsisError();
        }

		public string ip()
		{
			throw new NotImplementedException();
		}

		public void setError()
		{
			throw new NotImplementedException();
		}
	}

}
