using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC.Functions
{
	static class Memory
	{
		internal static float[] memory = new float[Int16.MaxValue];

		internal static float MemoryGet(Int16 pos) => memory[pos];
		internal static float MemorySet(Int16 pos, float value)
		{
			memory[pos] = value;
			return value;
		}


		[Serializable]
		public class BadMemoryAccess : Exception
		{
			public BadMemoryAccess() { }
			public BadMemoryAccess(string message) : base(message) { }
			public BadMemoryAccess(string message, Exception inner) : base(message, inner) { }
			protected BadMemoryAccess(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
	}


	class MemoryLoad : IFunction
	{
		float IFunction.Apply(List<BasicNumber> arguments)
		{
			float value = arguments[0].GetValue();
			if(value == (int)value && value >= 0 && value < Int16.MaxValue)
			{
				return Memory.MemoryGet((short)value);
			}
			throw new Memory.BadMemoryAccess("Could not access requested memory");
		}
	}
	class MemoryStore : IFunction
	{
		float IFunction.Apply(List<BasicNumber> arguments)
		{
			float value = arguments[0].GetValue();
			if (value == (int)value && value >= 0 && value < Int16.MaxValue)
			{
				return Memory.MemorySet((short)value, arguments[1]);
			}
			throw new Memory.BadMemoryAccess("Could not access requested memory");
		}
	}
}
