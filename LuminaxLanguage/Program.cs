using LuminaxLanguage.Processors;
using TextReader = LuminaxLanguage.Processors.TextReader;

var lexicalAnalyzer = new LexicalAnalyzer();
var currentState = 0;
var filePath = "C:\\Users\\kyrylo.sokyrka\\Repositories\\Personal\\LuminaxLanguage\\LuminaxLanguage\\test.txt";
try
{
    foreach (var lineOfText in TextReader.GetLineOfText(filePath))
    {
        lexicalAnalyzer.Analyze(lineOfText, ref currentState);
    }

    Console.WriteLine("Lexer: Lexical analyzer was successfully completed");
    Console.Write(lexicalAnalyzer.AnalysisInformation.SymbolsInformation);
    Console.WriteLine(lexicalAnalyzer.AnalysisInformation.Constants);
    Console.WriteLine(lexicalAnalyzer.AnalysisInformation.Ids);
}
catch (Exception e)
{
    var message = e.Message;
    var errorCode = message.Substring(0, 4);
    var mainMessage = message.Substring(2);
    Console.WriteLine(mainMessage);
    Console.WriteLine($"Lexer: Analysis failed with status {errorCode}");
}

//var bracketsProcessor = new BracketsProcessor();
//var syntaxAnalyzer = new SyntaxAnalyzer(lexicalAnalyzer.AnalysisInformation, bracketsProcessor);

//try
//{
//    syntaxAnalyzer.ParseProgram();
//}
//catch (Exception e)
//{
//    Console.Write(e.Message);
//}