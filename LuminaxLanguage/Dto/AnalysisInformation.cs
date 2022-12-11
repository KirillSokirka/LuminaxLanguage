
namespace LuminaxLanguage.Dto
{
    public class AnalysisInformation
    {
        public Dictionary<int, SymbolInformation> SymbolsInformation = new();
        public Dictionary<string, int> Ids = new();
        public Dictionary<string, int> Constants = new();
    }
}
