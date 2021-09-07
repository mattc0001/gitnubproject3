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
			arguments[0].runtime.pc = (int)arguments[0].GetOperand() - 2;
			return 0f;
		}
	}
	class JumpZero : IFunction
	{
		public float Apply(List<BasicNumber> arguments)
		{
			if(arguments[0] == 0)
				// Substract its own arity +1
				arguments[1].runtime.pc = (int)arguments[1].GetOperand() - 3;
			return 0f;
		}
	}
}
