using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuInterpreter.Parser
{
	public interface IMenuParser
	{
		ParseResult Parse(string filePath);
		Interpreter CreateInterpreterFromFile(string filePath);
	}
}
