using LNetwork.plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.service
{
	public interface SocketNetwork
	{
		uint SocketId();

		void Handle();

		uint SocketState();
	}
}
