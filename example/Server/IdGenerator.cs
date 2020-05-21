using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class IdGenerator
	{
		public uint Current
		{
			get; set;
		}

		public uint Next()
		{
			Current++;
			return Current;
		}

	}
}
