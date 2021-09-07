using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC.Functions
{
	class Multiply : IFunction
	{
		float IFunction.Apply(List<BasicNumber> arguments)
		{
			return arguments[0].GetValue() * arguments[1].GetValue();
		}
	}
}
