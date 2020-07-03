using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LNetwork
{
    public interface IDataSocket
    {
		string ip();
		void setError();
		bool isConnected();
		bool isError();
		void send(byte[] message);
		void handle();
		byte[] getMessage();
		void close();
    }
}
