using System;
using System.IO;

namespace SuperBASIC
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Library lib = new Library();
				lib.AddFunction(new Functions.Print(), 1, "PRINT");
				lib.AddFunction(new Functions.Multiply(), 2, "MULTIPLY");
				lib.AddFunction(new Functions.Compare(), 2, "COMPARE");
				lib.AddFunction(new Functions.Pi(), 0, "PI");
				lib.AddFunction(new Functions.Euler(), 0, "EULER");
				Runtime r = new Runtime(lib);
				r.OpenFile(Directory.GetCurrentDirectory() + "\\Test.basic");
				r.Run();
			} catch (Parser.ParseException e)
			{
				Console.WriteLine($"Parsing failed:\n{e}");
			}
		}
	}
}
