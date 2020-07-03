using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.plugins
{
	public class CombinedServerSocket: IServerSocket
	{
		List<IServerSocket> sockets = new List<IServerSocket>();

		public void Add(IServerSocket socket)
		{
			sockets.Add(socket);
		}

		public void Close()
		{
			foreach(var socket in sockets)
			{
				socket.Close();
			}
		}

		public IDataSocket handle()
		{
			foreach (var socket in sockets)
			{
				var dataSocket = socket.handle();
				if(dataSocket != null)
				{
					return dataSocket;
				}
			}
			return null;
		}

		public void listen(int port)
		{
			for(int i=0;i<sockets.Count; i++)
			{
				sockets[i].listen(port + i);
			}
		}
	}
}
