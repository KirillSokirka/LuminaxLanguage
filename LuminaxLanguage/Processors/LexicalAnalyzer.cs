using LuminaxLanguage.Constants;
using LuminaxLanguage.Dto;
using LuminaxLanguage.Tables;

namespace LuminaxLanguage.Processors
{
    public class LexicalAnalyzer
    {
        public int LineOfCode { get; set; } = 1;
        public AnalysisInformation AnalysisInformation { get; } = new();
        private int _counter;

        public void Analyze(string lineOfCode, ref int currentState)
        {
            var lexeme = string.Empty;

            for (; _counter <= lineOfCode.Length - 1; _counter++)
            {
                var symbolClass = SymbolAnalyzer.GetClassOfSymbol(lineOfCode[_counter]);

                currentState = GetState(currentState, symbolClass);

                CheckForErrors(currentState, lineOfCode[_counter]);

                ProcessStates(lineOfCode[_counter], ref lexeme, ref currentState);
            }

            _counter = 0;
            LineOfCode++;
        }

        private int GetState(int currentState, string symbolClass)
        {
            var state = -1;

            if (States.StateTransitionsDictionary
                .TryGetValue(new StateTransition(currentState, symbolClass), out var existingState))
            {
                state = existingState;
            }
            else if (States.StateTransitionsDictionary
                     .TryGetValue(new StateTransition(currentState, "other"), out var otherState))
            {
                state = otherState;
            }

            return state;
        }

        private void CheckForErrors(int currentState, char symbol)
        {
            switch (currentState)
            {
                case 101:
                    throw new Exception($"101 Lexer: in line {LineOfCode} unexpected symbol '{symbol}'");
                case 102:
                    throw new Exception($"102 Lexer: in line {LineOfCode} '=' was expected, received - '{symbol}'");
                case 103:
                    throw new Exception($"103 Lexer: in line {LineOfCode} Digit was expected, received - '{symbol}'");
                case 104:
                    throw new Exception($"104 Lexer: in line {LineOfCode} '-' or Digit were expected, received - '{symbol}'");
            }
        }

        private void ProcessStates(char symbol, ref string lexeme, ref int currentState)
        {
            if (States.FinalStates.Contains(currentState) || States.StatesToProcess.Contains(currentState))
            {
                ProcessFinalState(ref lexeme, currentState, symbol);

                currentState = States.InitState;
            }
            else if (currentState == States.InitState)
            {
                lexeme = string.Empty;
            }
            else
            {
                lexeme += symbol;
            }
        }

        private void ProcessFinalState(ref string lexeme, int currentState, char symbol)
        {
            var token = GetToken(lexeme, currentState, symbol);
            int? indexOfConstOrIndent = null;

            if (token == "nl")
            {
                lexeme = $"\\n";
                LineOfCode++;
            }
            else if (token == "ident")
            {
                if (!AnalysisInformation.Ids.ContainsKey(lexeme))
                {
                    indexOfConstOrIndent = AnalysisInformation.Ids.Count + 1;
                    AnalysisInformation.Ids.Add(lexeme, (int)indexOfConstOrIndent);
                }

                _counter--;
            }
            else if (new[] { "int", "float", "exp", "boolval" }.Contains(token))
            {
                if (!AnalysisInformation.Constants.ContainsKey(lexeme))
                {
                    indexOfConstOrIndent = AnalysisInformation.Ids.Count + 1;
                    AnalysisInformation.Constants.Add(lexeme, (int)indexOfConstOrIndent);
                }

                _counter--;
            }
            else if (new[] { "punct", "add_op", "mult_op", "power_op", "par_op", "rel_op" }.Contains(token))
            {
                lexeme += symbol;
            }

            var data = new SymbolInformation(LineOfCode, lexeme, token, indexOfConstOrIndent);
            AnalysisInformation.SymbolsInformation
                .Add(AnalysisInformation.SymbolsInformation.Count, data);

            lexeme = string.Empty;
        }

        private string GetToken(string lexeme, int state, char symbol)
        {
            var result = string.Empty;

            if (Tokens.LanguageTokens.TryGetValue(lexeme, out var languageToken))
            {
                result = languageToken;
            }
            else if (Tokens.OtherTokens.TryGetValue(state, out var numberToken))
            {
                result = numberToken;
            }
            else if (Tokens.LanguageTokens.TryGetValue(lexeme + symbol, out var relToken))
            {
                result = relToken;
            }
            else if (Tokens.LanguageTokens.TryGetValue(symbol.ToString(), out var symbolToken))
            {
                result = symbolToken;
            }

            return result;
        }
    }
}
