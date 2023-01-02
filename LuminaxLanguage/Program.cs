using LuminaxLanguage.Processors;

var lexicalAnalyzer = new LexicalAnalyzer();
var interpreter = new Interpreter(lexicalAnalyzer);

var filePath = "C:\\Users\\kyrylo.sokyrka\\Source\\Repos\\KirillSokirka\\LuminaxLanguage\\LuminaxLanguage\\test.txt";

interpreter.InterpretCode(filePath);
