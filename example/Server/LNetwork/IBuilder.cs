using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public interface IBuilder<T>
	{
		T Build();
	}
}
