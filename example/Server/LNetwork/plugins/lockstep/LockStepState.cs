using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.plugins.lockstep
{
	public interface LockStepState
	{
		void Tick();
		void ReadState(BinaryReader state);
		byte[] WriteState();
	}
}
