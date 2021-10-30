using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC
{
	class Runtime
	{
		public float register;
		internal Library lib;
		Bytecode code;
		internal int pc = 0;

		public Runtime(Library library)
		{
			lib = library;
		}

		public void OpenFile(string path)
		{
			code = new Parser(this).ParseFile(path);
		}

		public void Run()
		{
			for(pc = 0; pc < code.bytecode.Count;)
			{
				int opcode = code.bytecode[pc].GetOperand();
				int arity = lib.arities[opcode];
				IFunction op = lib.functions[opcode];
				var args = code.bytecode.GetRange(pc + 1, arity);
				SetRegister(op.Apply(args));
				pc += arity + 1;
			}
		}

		internal void SetRegister(float value)
		{
			register = value;
		}
		internal float GetRegister()
		{
			return register;
		}
	}
}
