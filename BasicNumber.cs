using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC
{
	enum NumberType{
		Ans,
		Number,
		Operand,
		Memory
	};

	struct BasicNumber
	{
		internal NumberType type;

		readonly Runtime runtime;
		readonly private float number;
		readonly private int operand;


		[Serializable]
		public class BadNumber : Exception
		{
			public BadNumber() { }
			public BadNumber(string message) : base(message) { }
			public BadNumber(string message, Exception inner) : base(message, inner) { }
			protected BadNumber(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

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
		internal BasicNumber(Runtime rt, int v, NumberType reqType = NumberType.Operand)
		{
			if(reqType == NumberType.Operand)
			{
				type = NumberType.Operand;
				number = 0;
				operand = v;
				runtime = rt;
			} 
			else
			{
				type = NumberType.Memory;
				number = 0;
				if (v > Int16.MaxValue) throw new BadNumber("Generated out of memory access");
				operand = v;
				runtime = rt;
			}
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
			else if(type == NumberType.Ans)
			{
				return runtime.GetRegister();
			}
#if MEMORY
			else
			{
				return Functions.Memory.MemoryGet((short)operand);
			}
#else
			return 0;
#endif
		}

		public static implicit operator float(BasicNumber v) => v.GetValue();
		public override string ToString() => $"{number}";
	}
}
