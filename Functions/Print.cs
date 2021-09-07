using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC.Functions
{
	class Print : IFunction
	{
		float IFunction.Apply(List<BasicNumber> arguments)
		{
			Console.WriteLine(arguments[0].GetValue());
			return 0f;
		}
	}
}
