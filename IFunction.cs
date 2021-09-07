using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC
{
	interface IFunction
	{
		public float Apply(List<BasicNumber> arguments);
	}
}
