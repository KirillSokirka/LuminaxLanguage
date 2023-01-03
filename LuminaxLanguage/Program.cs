using LuminaxLanguage.Processors;

var lexicalAnalyzer = new LexicalAnalyzer();
var interpreter = new Interpreter(lexicalAnalyzer);

const string filePath = "C:\\Users\\kyrylo.sokyrka\\Source\\Repos\\KirillSokirka\\LuminaxLanguage\\LuminaxLanguage\\test.txt";

interpreter.InterpretCode(filePath);
