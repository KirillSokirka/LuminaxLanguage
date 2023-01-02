namespace LuminaxLanguage.Dto
{
    public class AnalysisInformation
    {
        public Dictionary<int, SymbolInformation> SymbolsInformation = new();
        public Dictionary<string, ValueContainer> Ids = new();
        public Dictionary<string, ValueContainer> Constants = new();
    }

    public record ValueContainer(int IdInTable, Type? Type, object? Value);
}
