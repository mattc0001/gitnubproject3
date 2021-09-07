using System;
using System.IO;

namespace SuperBASIC
{
	class Program
	{
		static void Main(string[] args)
		{
			Library lib = new Library();
			lib.AddFunction(new Functions.Print(), 1, "PRINT");
			lib.AddFunction(new Functions.Multiply(), 2, "MULTIPLY");
			Runtime r = new Runtime(lib);
			r.OpenFile(Directory.GetCurrentDirectory() + "\\Test.basic");
			r.Run();
		}
	}
}
