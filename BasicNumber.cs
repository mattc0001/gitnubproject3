using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC
{
	enum NumberType{
		Ans,
		Number,
		Operand
	};

	struct BasicNumber
	{
		internal NumberType type;

		readonly Runtime runtime;
		readonly private float number;
		readonly private int operand;

		internal BasicNumber(Runtime rt, float v)
		{
			type = NumberType.Number;
			number = v;
			operand = 0;
			runtime = rt;
		}
		internal BasicNumber(Runtime rt)
		{
			type = NumberType.Ans;
			number = 0;
			operand = 0;
			runtime = rt;
		}
		internal BasicNumber(Runtime rt, int v)
		{
			type = NumberType.Operand;
			number = 0;
			operand = v;
			runtime = rt;
		}

		internal int GetOperand()
		{
			return operand;
		}

		public float GetValue()
		{
			if (type == NumberType.Number)
			{
				return number;
			} 
			else
			{
				return runtime.GetRegister();
			}
		}

		public static implicit operator float(BasicNumber v) => v.GetValue();
		public override string ToString() => $"{number}";
	}
}
