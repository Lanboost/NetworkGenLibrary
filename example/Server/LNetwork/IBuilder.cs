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

	public class BuilderWrapper<T>: IBuilder<T>
	{
		Func<T> func;
		public BuilderWrapper(Func<T> func)
		{
			this.func = func;
		}

		public T Build()
		{
			return func();
		}
	}
}
