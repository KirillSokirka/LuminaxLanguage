using LuminaxLanguage.Constants;
using LuminaxLanguage.Dto;

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
                    throw new Exception("Parser: ");
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
                    _bracketsProcessor.ControlBracketsFlow(AnalysisInformation[Iterator], "{");

                    ProcessDeclarationSection();

                    ProcessDoSection();

                    _bracketsProcessor.ControlBracketsFlow(AnalysisInformation[Iterator], "}");

                    _bracketsProcessor.CheckStackStatus();
                }
                else
                {
                    throw new Exception("Parser: unexpected error");
                }
            }

            return resultLists;
        }

        #region Token Parsers

        private static bool ParseToken((string, string) expectedToken, SymbolInformation currentInformation)
        {
            if (expectedToken == (currentInformation.Lexeme, currentInformation.LexemeToken))
            {
                Console.WriteLine(ParserMessages.Information,
                    currentInformation.LineNumber,
                    currentInformation.Lexeme,
                    currentInformation.LexemeToken);
            }
            else
            {
                throw new Exception(string.Format(ParserMessages.ErrorWithUnexpectedElements,
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
                Console.WriteLine(ParserMessages.Information,
                    currentInformation.LineNumber,
                    currentInformation.Lexeme,
                    currentInformation.LexemeToken);
            }
            else
            {
                throw new Exception(string.Format(ParserMessages.ErrorWithUnexpectedToken,
                    currentInformation.LineNumber,
                    currentInformation.LexemeToken,
                    expectedToken));
            }

            return true;
        }

        private static bool ParseIdentToken(SymbolInformation symbolInformation) =>
            symbolInformation.LexemeToken == "ident" && ParseToken("ident", symbolInformation);

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
            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "{");

            var symbolInformation = ProcessIdentList();

            _bracketsProcessor.ControlBracketsFlow(symbolInformation, "}");
        }

        private SymbolInformation ProcessIdentList()
        {
            SymbolInformation symbolInformation;

            while ((symbolInformation = AnalysisInformation![Iterator]).Lexeme != "}")
            {
                if (ParseTypeToken(symbolInformation))
                {
                    symbolInformation = AnalysisInformation[Iterator];

                    symbolInformation = ParseIdentList(symbolInformation, ";");

                    ParseToken((";", "punct"), symbolInformation);
                }
            }

            return symbolInformation;
        }

        private SymbolInformation ParseIdentList(SymbolInformation symbolInformation, string endOfIdent)
        {
            if (ParseIdentToken(symbolInformation))
            {
                symbolInformation = AnalysisInformation![Iterator];

                while (symbolInformation.Lexeme != endOfIdent)
                {
                    if (ParseToken((",", "punct"), symbolInformation))
                    {
                        ParseIdentToken(AnalysisInformation![Iterator]);
                    }

                    symbolInformation = AnalysisInformation[Iterator];
                }
            }

            return symbolInformation;
        }

        private void ProcessDoSection()
        {
            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "{");

            while (ProcessStatementList()) { }

            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![_iterator], "}");
        }

        private bool ProcessStatementList()
        {
            var symbolInformation = AnalysisInformation![Iterator];

            var result = symbolInformation switch
            {
                { LexemeToken: "ident" } => ProcessIdentExpression(symbolInformation),
                { LexemeToken: "keyword", Lexeme: "input" } => ProcessInputExpression(symbolInformation),
                { LexemeToken: "keyword", Lexeme: "print" } => ProcessPrintExpression(symbolInformation),
                { LexemeToken: "keyword", Lexeme: "if" } => ProcessIfExpression(symbolInformation),
                { LexemeToken: "keyword", Lexeme: "do" } => ProcessDoWhileExpression(symbolInformation),
                { LexemeToken: "par_op", Lexeme: "}" } => false,
                _ => throw new Exception($"Parser: unexpected token {symbolInformation.Lexeme}")
            };

            return result;
        }

        private bool ProcessIdentExpression(SymbolInformation symbolInformation)
        {
            var result = false;

            if (ParseIdentToken(symbolInformation))
            {
                symbolInformation = AnalysisInformation![Iterator];

                if (ParseToken(("=", "assign_op"), symbolInformation))
                {
                    result = ParseExpression();

                    if (result)
                    {
                        result = ParseToken((";", "punct"), AnalysisInformation![_iterator]);
                    }
                }
                else
                {
                    throw new Exception(string.Format(ParserMessages.ErrorExpectedAssignToken,
                        symbolInformation.LineNumber));
                }
            }

            return result;
        }

        private bool ParseExpression()
        {
            var temp = _iterator;

            if (ParseBooleanExpression())
            {
                return true;
            }

            Iterator = temp;

            if (ParseArithmeticExpression(AnalysisInformation![Iterator]))
            {
                return true;
            }

            throw new Exception("bebra not allowed");
        }

        private bool ParseBooleanExpression()
        {
            var symbol = AnalysisInformation![Iterator];

            if (symbol.Lexeme is "true" or "false" && ParseToken("boolval", symbol))
            {
                return true;
            }

            if (symbol.Lexeme is "(" && _bracketsProcessor.ControlBracketsFlow(symbol, "("))
            {
                if (ParseArithmeticExpression(AnalysisInformation[Iterator]))
                {
                    symbol = AnalysisInformation[_iterator];
                    if (ParseRelExpression(symbol))
                    {
                        symbol = AnalysisInformation[Iterator];
                        if (ParseArithmeticExpression(symbol))
                        {
                            if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation[_iterator], ")"))
                            {
                                _iterator++;
                                return true;
                            }
                        }
                    }
                }
            }
            else if (ParseArithmeticExpression(symbol))
            {
                symbol = AnalysisInformation[_iterator];
                if (ParseRelExpression(symbol))
                {
                    symbol = AnalysisInformation[Iterator];
                    if (ParseArithmeticExpression(symbol))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ParseRelExpression(SymbolInformation symbolInformation)
            => symbolInformation.LexemeToken is "rel_op" && ParseToken("rel_op", symbolInformation);

        private bool ParseArithmeticExpression(SymbolInformation symbolInformation)
        {
            if (symbolInformation.Lexeme is "+" or "-")
                ParseToken("add_op", symbolInformation);

            if (ParseTerm(symbolInformation))
            {
                var symbol = AnalysisInformation![_iterator];

                while (symbol.Lexeme is "+" or "-" && ParseToken("add_op", symbol))
                {
                    if (!ParseTerm(AnalysisInformation[Iterator]))
                        return false;

                    symbol = AnalysisInformation![_iterator];
                }

                return true;
            }

            return false;
        }

        private bool ParseTerm(SymbolInformation symbolInformation)
        {
            if (ParseChunk(symbolInformation))
            {
                symbolInformation = AnalysisInformation![_iterator];

                while (symbolInformation.Lexeme is "*" or "/" && ParseToken("mult_op", symbolInformation))
                {
                    if (!ParseChunk(AnalysisInformation[Iterator]))
                        return false;

                    symbolInformation = AnalysisInformation![_iterator];
                }

                return true;
            }

            return false;
        }

        private bool ParseChunk(SymbolInformation symbolInformation)
        {
            if (ParseFactor(symbolInformation))
            {
                var currentInformation = AnalysisInformation![_iterator];

                while (currentInformation.Lexeme == "^" && ParseToken(("^", "pow_op"), currentInformation))
                {
                    if (!ParseFactor(AnalysisInformation![Iterator]))
                    {
                        return false;
                    }

                    currentInformation = AnalysisInformation![_iterator];
                }

                return true;
            }

            return false;
        }

        private bool ParseFactor(SymbolInformation symbol)
        {
            if (ParseConst(symbol) || ParseIdentToken(symbol))
            {
                _iterator++;
                return true;
            }

            if (symbol.Lexeme is "(" && _bracketsProcessor.ControlBracketsFlow(symbol, "("))
            {
                if (ParseArithmeticExpression(AnalysisInformation![Iterator]))
                {
                    return _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], ")");
                }
            }

            return false;
        }

        private bool ParseConst(SymbolInformation symbol)
            => (symbol.LexemeToken is "boolval" && ParseToken("boolval", symbol)) ||
                (symbol.LexemeToken is "int" && ParseNumber(symbol, "int")) ||
                (symbol.LexemeToken is "exp" && ParseNumber(symbol, "exp")) ||
                (symbol.LexemeToken is "float" && ParseNumber(symbol, "float"));

        private bool ParseNumber(SymbolInformation symbol, string typeOfNumber)
        {
            if (symbol.Lexeme is "+" or "-")
            {
                if (!ParseToken("add_op", symbol))
                    return false;
            }

            return ParseToken(typeOfNumber, symbol);
        }

        private bool ProcessDoWhileExpression(SymbolInformation symbolInformation)
        {
            if (ParseToken(("do", "keyword"), symbolInformation))
            {
                if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "{"))
                {
                    while (ProcessStatementList()) { }

                    if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation![_iterator], "}"))
                    {
                        if (ParseToken(("while", "keyword"), AnalysisInformation![Iterator]))
                        {
                            if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation[Iterator], "("))
                            {
                                if (ParseBooleanExpression())
                                {
                                    return _bracketsProcessor.ControlBracketsFlow(AnalysisInformation[_iterator], ")");
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool ProcessIfExpression(SymbolInformation symbolInformation)
        {
            if (ParseToken(("if", "keyword"), symbolInformation))
            {
                if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "("))
                {
                    if (ParseBooleanExpression())
                    {
                        if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation![_iterator], ")"))
                        {
                            _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "{");

                            while (ProcessStatementList()) { }

                            return _bracketsProcessor.ControlBracketsFlow(AnalysisInformation![_iterator], "}");
                        }
                    }
                }
            }

            return false;
        }

        private bool ProcessPrintExpression(SymbolInformation symbolInformation)
        {
            if (ParseToken(("print", "keyword"), symbolInformation))
            {
                if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "("))
                {
                    symbolInformation = ParseIdentList(AnalysisInformation[Iterator], ")");

                    if (_bracketsProcessor.ControlBracketsFlow(symbolInformation, ")"))
                    {
                        return ParseToken((";", "punct"), AnalysisInformation[Iterator]);
                    }
                }
            }

            return false;
        }

        private bool ProcessInputExpression(SymbolInformation symbolInformation)
        {
            if (ParseToken(("input", "keyword"), symbolInformation))
            {
                if (_bracketsProcessor.ControlBracketsFlow(AnalysisInformation![Iterator], "("))
                {
                    symbolInformation = ParseIdentList(AnalysisInformation[Iterator], ")");

                    if (_bracketsProcessor.ControlBracketsFlow(symbolInformation, ")"))
                    {
                        return ParseToken((";", "punct"), AnalysisInformation[Iterator]);
                    }
                }
            }

            return false;
        }

        #endregion
    }
}