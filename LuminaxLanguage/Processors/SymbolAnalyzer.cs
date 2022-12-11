using LuminaxLanguage.Constants;

namespace LuminaxLanguage.Processors
{
    public static class SymbolAnalyzer
    {
        public static string GetClassOfSymbol(char charSymbol)
        {
            var symbol = charSymbol.ToString();
            return symbol switch
            {
                { } s when SymbolClass.LetterExample.Contains(s) => SymbolClass.Letter,
                { } s when SymbolClass.DigitExample.Contains(s) => SymbolClass.Digit,
                { } s when SymbolClass.WhiteSpacesExample.Contains(s) => SymbolClass.WhiteSpaces,
                { } s when SymbolClass.NewLineExample.Contains(s) => SymbolClass.NewLine,
                { } s when SymbolClass.OtherExample.Contains(s) => s,
                _ => "symbol doesn't belongs to alphabet"
            };
        }
    }
}
