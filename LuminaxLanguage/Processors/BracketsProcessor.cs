using LuminaxLanguage.Constants;
using LuminaxLanguage.Dto;

namespace LuminaxLanguage.Processors;

public class BracketsProcessor
{
    // ReSharper disable once InconsistentNaming
    private Stack<string> BracketsStack = new(4);

    public bool ControlBracketsFlow(SymbolInformation bracket, string expectedBracket)
    {
        var result = false;

        if (bracket.LexemeToken == "par_op" && bracket.Lexeme == expectedBracket)
        {
            Console.WriteLine(ParserMessages.Information, bracket.LineNumber, bracket.Lexeme, bracket.LexemeToken);
            
            if (bracket.Lexeme is "{" or "(")
            {
                BracketsStack.Push(bracket.Lexeme);
                result = true;
            }
            else if (BracketsStack.TryPop(out var bracketInStack))
            {
                if ((bracketInStack == "{" && bracket.Lexeme == "}") ||
                    (bracketInStack == "(" && bracket.Lexeme == ")"))
                {
                    result = true;
                }
            }
        }

        if (!result)
        {
            throw new Exception($"Parser: unexpected bracket '{bracket.Lexeme}', expected - {expectedBracket}");
        }

        return result;
    }

    public void CheckStackStatus()
    {
        if (BracketsStack.Count != 0)
        {
            throw new Exception("Parser: some brackets wasn't closed");
        }
    }
}