namespace MenuInterpreter.Parser
{
    public interface IMenuParser
    {
        Interpreter CreateInterpreterFromFile(string filePath);

        ParseResult Parse(string filePath);
    }
}