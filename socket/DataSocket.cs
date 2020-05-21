using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace OribiosNetwork.network
{
    public abstract class DataSocket
    {
		
		public abstract string ip();
		public abstract void setError();
		public abstract bool isConnected();
		public abstract bool isError();
		public abstract void send(byte[] message);
		public abstract void handle();
		public abstract byte[] getMessage();
		public abstract void close();
    }
}
