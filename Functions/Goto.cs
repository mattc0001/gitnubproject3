using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC.Functions
{
	class Goto : IFunction
	{
		public float Apply(List<BasicNumber> arguments)
		{
			// Substract its own arity +1
			arguments[0].runtime.pc = (int)arguments[0] - 2;
			return arguments[0].runtime.register;
		}
	}
	class JumpZero : IFunction
	{
		public float Apply(List<BasicNumber> arguments)
		{
			if(arguments[0] == 0)
				// Substract its own arity +1
				arguments[1].runtime.pc = (int)arguments[1] - 3;
			return arguments[0].runtime.register;
		}
	}
}
