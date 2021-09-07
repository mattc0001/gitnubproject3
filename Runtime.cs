using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC
{
	class Runtime
	{
		float register;
		internal Library lib;
		Bytecode code;

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
			for(int idx = 0; idx < code.bytecode.Count;)
			{
				int opcode = code.bytecode[idx].GetOperand();
				int arity = lib.arities[opcode];
				IFunction op = lib.functions[opcode];
				var args = code.bytecode.GetRange(idx + 1, arity);
				SetRegister(op.Apply(args));
				idx += arity + 1;
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
