using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public class UIDCounter
	{
		uint Current = 0;
		public uint Get()
		{
			Current++;
			return Current;
		}
	}
}
