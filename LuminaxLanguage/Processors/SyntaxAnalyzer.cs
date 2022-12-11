using LuminaxLanguage.Constants;
using LuminaxLanguage.Dto;
using LuminaxLanguage.Tables;

namespace LuminaxLanguage.Processors
{
    public class SyntaxAnalyzer
    {
        // ReSharper disable once InconsistentNaming
        private readonly List<SymbolInformation>? AnalysisInformation;
        private readonly BracketsProcessor _bracketsProcessor;
        private int _iterator;

        private int Iterator
        {
            get
            {
                if (++_iterator > AnalysisInformation!.Count - 1)
                    throw new Exception("error");
                return _iterator;
            }
            set => _iterator = value;
        }

        public SyntaxAnalyzer(AnalysisInformation analysisInformation, BracketsProcessor bracketsProcessor)
        {
            AnalysisInformation = analysisInformation.SymbolsInformation.Values.ToList();
            _bracketsProcessor = bracketsProcessor;
            _iterator = -1;
        }

        public List<string> ParseProgram()
        {
            var resultLists = new List<string>();

            if (AnalysisInformation is not null)
            {
                if (ProcessProgramStartSection())
                {
                    _bracketsProcessor.ControlBracketsFlow(AnalysisInformation[Iterator]);

                    ProcessDeclarationSection();

                    ProcessDoSection();

                    _bracketsProcessor.ControlBracketsFlow(AnalysisInformation[Iterator]);

                    _bracketsProcessor.CheckStackStatus();
                }
                else
                {
                    throw new Exception("");
                }
            }

            return resultLists;
        }

        #region Token Parsers

        private static bool ParseToken((string, string) expectedToken, SymbolInformation currentInformation)
        {
            if (expectedToken == (currentInformation.Lexeme, currentInformation.LexemeToken))
            {
                Console.WriteLine(SyntaxAnalyzerMessageTemplates.ParseInformation,
                    currentInformation.LineNumber,
                    currentInformation.Lexeme,
                    currentInformation.LexemeToken);
            }
            else
            {
                throw new Exception(string.Format(SyntaxAnalyzerMessageTemplates.ParseErrorWithUnexpectedElements,
                    currentInformation.LineNumber,
                    currentInformation.Lexeme,
                    currentInformation.LexemeToken,
                    expectedToken.Item1,
                    expectedToken.Item2));
            }

            return true;
        }

        private static bool ParseToken(string expectedToken, SymbolInformation currentInformation)
        {
            if (expectedToken == currentInformation.LexemeToken)
            {
                Console.WriteLine(SyntaxAnalyzerMessageTemplates.ParseInformation,
                    currentInformation.LineNumber,
                    currentInformation.Lexeme,
                    currentInformation.LexemeToken);
            }
            else
            {
                throw new Exception(string.Format(SyntaxAnalyzerMessageTemplates.ParseErrorWithUnexpectedToken,
                    currentInformation.LineNumber,
                    currentInformation.LexemeToken,
                    expectedToken));
            }

            return true;
        }

        private static bool ParseIdentToken(SymbolInformation symbolInformation) =>
            ParseToken("ident", symbolInformation);

        private static bool ParseTypeToken(SymbolInformation symbolInformation) =>
            new[] { "int", "float", "boolean" }.Contains(symbolInformation.Lexeme) &&
            ParseToken("keyword", symbolInformation);

        #endregion

        #region Main Parts Processors

        private bool ProcessProgramStartSection()
        {
            var result = false;

            var symbolInformation = AnalysisInformation![Iterator];

            if (ParseToken(("program", "keyword"), symbolInformation))
            {
                symbolInformation = AnalysisInformation![Iterator];

                if (ParseIdentToken(symbolInformation))
                {
                    result = true;
                }
            }

            return result;
        }

        private void ProcessDeclarationSection()
        {
            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator]);

            var symbolInformation = ProcessIdentList();

            _bracketsProcessor.ControlBracketsFlow(symbolInformation);
        }

        private SymbolInformation ProcessIdentList()
        {
            SymbolInformation symbolInformation;

            while ((symbolInformation = AnalysisInformation![Iterator]).Lexeme != "}")
            {
                if (ParseTypeToken(symbolInformation))
                {
                    symbolInformation = AnalysisInformation[Iterator];

                    if (ParseIdentToken(symbolInformation))
                    {
                        symbolInformation = AnalysisInformation[Iterator];

                        while (symbolInformation.Lexeme != ";")
                        {
                            if (ParseToken((",", "punct"), symbolInformation))
                            {
                                ParseIdentToken(AnalysisInformation![Iterator]);
                            }

                            symbolInformation = AnalysisInformation[Iterator];
                        }
                    }

                    ParseToken((";", "punct"), symbolInformation);
                }
            }

            return symbolInformation;
        }

        private void ProcessDoSection()
        {
            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator]);


            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation[Iterator]);
        }

        #endregion
    }
}