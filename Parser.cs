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
		Dictionary<string, int> namedLabels;
		List<Tuple<string, int>> positionedLabels;
		public Parser(Runtime rt)
		{
			runtime = rt;
			library = rt.lib;
		}

		internal enum Controls
		{
			If, Then, Else, EndIf,
			For, To, Step, Next,
			While, Wend,
			Repeat, Until,
			Do, LoopCond
		}

		internal struct FlowControlTag
		{
			public Controls ctrl;
			public int index;
		}

		private void ParseExpression(Bytecode c, string[] components, int idx, List<int> lineSpans)
		{
			if (library.nameResolution.ContainsKey(components[0]))
			{

				int opcode = library.nameResolution[components[0]];
				int arity = library.arities[opcode];

				if (arity != components.Length - 1)
				{
					int lineIndex = 0;
					foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
					throw new ParseException($"Operation {components[0]} was provided with the wrong number of arguments\n\tExpected {arity} found {components.Length - 1}\n\tat line {lineIndex}");
				}

				c.bytecode.Add(new BasicNumber(runtime, opcode));
				foreach (string elem in components.AsSpan(1))
				{
#if MEMORY
					if (elem.StartsWith("M"))
					{
						try
						{
							Int16 v = Int16.Parse(elem[1..]);
							c.bytecode.Add(new BasicNumber(runtime, v, NumberType.Memory));
						}
						catch (Exception)
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Cannot parse {elem} as argument to memory address\n\tExpected 'M' followed by an integer lower than {Int16.MaxValue}\n\tat line {lineIndex}");
						}
					}
					else
#endif
					if (namedLabels.ContainsKey(elem))
					{
						positionedLabels.Add(new Tuple<string, int>(elem, c.bytecode.Count));
						c.bytecode.Add(new BasicNumber(runtime, (float)namedLabels[elem]));
					}
					else if (elem == "$")
					{
						c.bytecode.Add(new BasicNumber(runtime));
					}
					else
					{
						try
						{
							float v = float.Parse(elem);
							c.bytecode.Add(new BasicNumber(runtime, v));
						}
						catch (Exception)
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Cannot parse {elem} as argument\n\tExpected floating point number or '$' or memory argument\n\tat line {lineIndex}");
						}

					}
				}
			}
			else
			{
				int lineIndex = 0;
				foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
				throw new ParseException($"Unknown operation \"{components[0]}\"\n\tat line {lineIndex}");
			}
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
			Stack<FlowControlTag> labelStack = new Stack<FlowControlTag>();
			namedLabels = new Dictionary<string, int>();
			positionedLabels = new List<Tuple<string, int>>();

			int a = 0;
			foreach (string line in sourceLines)
			{
				a++;
				string l = lws.Replace(line, " ");
				l = leadings.Replace(l, "");
				l = trailings.Replace(l, "");

				if (l != String.Empty)
				{
					lineSpans.Add(a);
					a = 0;
					codeLines.Add(l);
				}
			}


			for (int idx = 0; idx < codeLines.Count; idx++)
			{
				string line = codeLines[idx];
				var components = line.Split(' ');
				if (components[0] == "LABEL")
				{
					if (components.Length != 2)
					{
						int lineIndex = 0;
						foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
						throw new ParseException($"Bad LABEL statement\n\tat line {lineIndex}");
					}
					namedLabels[components[1]] = -1;
				}
			}

			for (int idx = 0; idx < codeLines.Count; idx++)
			{
				string line = codeLines[idx];
				var components = line.Split(' ');

				var label = new FlowControlTag();
				switch (components[0])
				{
					case "IF":
						// Role: manages the expression
						label.ctrl = Controls.If;
						label.index = c.bytecode.Count;
						labelStack.Push(label);
						ParseExpression(c, components[1..], idx, lineSpans);
						break;
					case "LABEL":
						if (components.Length != 2)
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Bad LABEL statement\n\tat line {lineIndex}");
						}
						namedLabels[components[1]] = c.bytecode.Count;
						break;
					case "THEN":
						// Role: Jumps to the far block if false
						// 1. Adds the label to update the destination
						label.ctrl = Controls.Then;
						label.index = c.bytecode.Count;
						labelStack.Push(label);
						// 2. Adds the jumper
						c.bytecode.Add(new BasicNumber(runtime, library.nameResolution["JZ"]));
						c.bytecode.Add(new BasicNumber(runtime));
						c.bytecode.Add(new BasicNumber(runtime, 0f));
						break;
					case "ELSE":
						//Role: skips its section if encountered, marks the THEN for JZ
						//1. Gets the THEN label
						label = labelStack.Pop();
						if(label.ctrl != Controls.Then)
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Orphaned ELSE\n\tat line {lineIndex}");
						}
						//2. Self tags for the ENDIF to complete the GOTO
						var label_self = new FlowControlTag
						{
							ctrl = Controls.Else,
							index = c.bytecode.Count
						};
						labelStack.Push(label_self);
						//3. Pushes the GOTO
						c.bytecode.Add(new BasicNumber(runtime, library.nameResolution["GOTO"]));
						c.bytecode.Add(new BasicNumber(runtime, 0f));
						//4. Place the THEN destination after the GOTO
						c.bytecode[label.index+2] = new BasicNumber(runtime, (float)c.bytecode.Count);
						break;
					case "ENDIF":
						//Role: Destination if depending on what is skipped
						label = labelStack.Pop();
						if (label.ctrl == Controls.Else)
						{
							//Case 1: update the GOTO
							c.bytecode[label.index + 1] = new BasicNumber(runtime, (float)c.bytecode.Count);
						}
						else if (label.ctrl == Controls.Then)
						{
							//Case 2: update the JZ
							c.bytecode[label.index + 2] = new BasicNumber(runtime, (float)c.bytecode.Count);
						}
						else
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Orphaned ENDIF\n\tat line {lineIndex}");
						}
						// Controls the syntax
						if (labelStack.Pop().ctrl != Controls.If)
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Orphaned ENDIF despite THEN\n\tat line {lineIndex}");
						}
						break;
					case "WHILE":
						// Role: manages the expression
						// 1. Deal with expression
						label.ctrl = Controls.While;
						label.index = c.bytecode.Count;
						labelStack.Push(label);
						ParseExpression(c, components[1..], idx, lineSpans);
						// 2. Adds the label to update the destination
						label.ctrl = Controls.Wend;
						label.index = c.bytecode.Count;
						labelStack.Push(label);
						// 3. Adds the jumper
						c.bytecode.Add(new BasicNumber(runtime, library.nameResolution["JZ"]));
						c.bytecode.Add(new BasicNumber(runtime));
						c.bytecode.Add(new BasicNumber(runtime, 0f));
						break;
					case "WEND":
						//Role: Destination setting and loop
						var labelA = labelStack.Pop();
						if (labelA.ctrl == Controls.Wend)
						{

							label = labelStack.Pop();
							if (label.ctrl == Controls.While)
							{
								c.bytecode.Add(new BasicNumber(runtime, library.nameResolution["GOTO"]));
								c.bytecode.Add(new BasicNumber(runtime, (float)label.index));
								c.bytecode[labelA.index + 2] = new BasicNumber(runtime, (float)c.bytecode.Count);
							}
						}
						else
						{
							int lineIndex = 0;
							foreach (int cnt in lineSpans.GetRange(0, idx + 1)) lineIndex += cnt;
							throw new ParseException($"Orphaned WEND\n\tat line {lineIndex}");
						}
						break;
					default:
						ParseExpression(c, components, idx, lineSpans);
					break;
				}
			}

			foreach(var lbl in positionedLabels)
			{
				c.bytecode[lbl.Item2] = new BasicNumber(runtime, (float)namedLabels[lbl.Item1]);
			}
			return c;
		}
	}
}
