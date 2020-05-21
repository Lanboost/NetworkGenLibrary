using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public abstract class GameInstance
	{
		public abstract void OnStart(uint[] players, string[] names);

		public abstract void OnLeave(uint connectionId);

		public abstract void OnTick();

		public abstract void Init();

		public abstract bool IsDone();

		public Random Random
		{
			get; set;
		}

		public int Self
		{
			get; set;
		}
	}
}
