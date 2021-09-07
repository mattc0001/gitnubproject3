using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBASIC
{
	class Library
	{
		internal List<IFunction> functions;
		internal List<int> arities;
		internal Dictionary<string, int> nameResolution;

		public Library()
		{
			functions = new List<IFunction>();
			arities = new List<int>();
			nameResolution = new Dictionary<string, int>();
		}

		public int AddFunction(IFunction fn, int arity, string name)
		{
			int idx = functions.Count;
			functions.Add(fn);
			arities.Add(arity);
			nameResolution[name] = idx;
			return idx;
		}
	}
}
