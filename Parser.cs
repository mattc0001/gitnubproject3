using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SuperBASIC
{
	class Parser
	{

		[Serializable]
		public class ParseException : Exception
		{
			public ParseException() { }
			public ParseException(string message) : base(message) { }
			public ParseException(string message, Exception inner) : base(message, inner) { }
			protected ParseException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		readonly Library library;
		readonly Runtime runtime;
		public Parser(Runtime rt)
		{
			runtime = rt;
			library = rt.lib;
		}

		public Bytecode ParseFile(string filepath)
		{
			Bytecode c = new Bytecode();
			string[] sourceLines = File.ReadAllLines(filepath);
			Regex lws = new Regex(@"\s+");
			Regex leadings = new Regex(@"^\s+");
			Regex trailings = new Regex(@"\s+$");
			List<string> codeLines = new List<string>();
			List<int> lineSpans = new List<int>();

			int a = 0;
			foreach (string line in sourceLines)
			{
				a++;
				string l = lws.Replace(line, " ");
				l = leadings.Replace(l, "");
				l = trailings.Replace(l, "");

				if(l != String.Empty)
				{
					lineSpans.Add(a);
					a = 0;
					codeLines.Add(l);
				}
			}

			for(int idx = 0; idx < codeLines.Count; idx++)
			{
				string line = codeLines[idx];
				var components = line.Split(' ');

				if(!library.nameResolution.ContainsKey(components[0]))
				{
					int lineIndex = 0;
					foreach (int cnt in lineSpans.GetRange(0, idx)) lineIndex += cnt;
					throw new ParseException($"Unknown operation \"{components[0]}\"\n\tat line {lineIndex}");
				}

				int opcode = library.nameResolution[components[0]];
				int arity = library.arities[opcode];

				if(arity != components.Length-1)
				{
					int lineIndex = 0;
					foreach (int cnt in lineSpans.GetRange(0, idx)) lineIndex += cnt;
					throw new ParseException($"Operation {components[0]} was provided with the wrong number of arguments\n\tExpected {arity} found {components.Length-1}\n\tat line {lineIndex}");
				}

				c.bytecode.Add(new BasicNumber(runtime, opcode));
				foreach (string elem in components.AsSpan(1))
				{
					if (elem != "$")
					{
						c.bytecode.Add(new BasicNumber(runtime, float.Parse(elem)));
					}
					else
					{
						c.bytecode.Add(new BasicNumber(runtime));
					}
				}
			}

			return c;
		}
	}
}
