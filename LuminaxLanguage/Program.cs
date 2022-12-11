using LuminaxLanguage.Processors;
using TextReader = LuminaxLanguage.Processors.TextReader;

var lexicalAnalyzer = new LexicalAnalyzer();
try
{
    var currentState = 0;
    var filePath = "C:\\Users\\kyrylo.sokyrka\\Source\\Repos\\KirillSokirka\\LuminaxLanguage\\LuminaxLanguage\\test.txt";
    foreach (var lineOfText in TextReader.GetLineOfText(filePath))
    {
        lexicalAnalyzer.Analyze(lineOfText, ref currentState);

        //foreach (var symbolInfo in lexicalAnalyzer.AnalysisInformation.SymbolsInformation.Values)
        //{
        //    Console.WriteLine($"{symbolInfo.LineNumber}\t{symbolInfo.Lexeme}\t{symbolInfo.LexemeToken}\t{symbolInfo.Index}");
        //}
    }

    Console.WriteLine("Lexer: Lexical analyzer was successfully completed");
}
catch (Exception e)
{
    var message = e.Message;
    var errorCode = message.Substring(0, 4);
    var mainMessage = message.Substring(2);
    Console.WriteLine(mainMessage);
    Console.WriteLine($"Lexer: Analysis failed with status {errorCode}");
}

var bracketsProcessor = new BracketsProcessor();
var syntaxAnalyzer = new SyntaxAnalyzer(lexicalAnalyzer.AnalysisInformation, bracketsProcessor);
syntaxAnalyzer.ParseProgram();