using LuminaxLanguage.Dto;

namespace LuminaxLanguage.Processors;

public class BracketsProcessor
{
    // ReSharper disable once InconsistentNaming
    private Stack<string> BracketsStack = new(4);

    public void ControlBracketsFlow(SymbolInformation bracket)
    {
        var result = false;

        if (bracket.LexemeToken == "par_op")
        {
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
            throw new Exception("erorr,");
        }
    }

    public void CheckStackStatus()
    {
        if (BracketsStack.Count == 0)
        {
            throw new Exception("eror with brackets");
        }
    }
}