namespace MenuInterpreter.Parser
{
    public interface IMenuParser
    {
        ParseResult Parse(string filePath);
        Interpreter CreateInterpreterFromFile(string filePath);
    }
}