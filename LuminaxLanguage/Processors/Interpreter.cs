using LuminaxLanguage.Constants;
using LuminaxLanguage.Dto;

namespace LuminaxLanguage.Processors
{
    public class Interpreter
    {
        private readonly LexicalAnalyzer _lexicalAnalyzer;

        private AnalysisInformation? _analysisInformation;
        private List<TokenInformation> _postfixCode;
        private SyntaxAnalyzer? _syntaxAnalyzer;
        private Stack<TokenInformation> _stack = new();
        private int _iterator;

        public Interpreter(LexicalAnalyzer lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }

        public void InterpretCode(string filePath)
        {
            if (TranslateCode(filePath))
            {
                try
                {
                    var result = Interpret();

                    if (result)
                    {
                        Console.WriteLine("Interpreter: program has finished successfully");
                    }
                }
                catch (InterpreterException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private bool TranslateCode(string filePath)
        {
            try
            {
                var currentState = 0;

                foreach (var lineOfText in TextReader.GetLineOfText(filePath))
                {
                    _lexicalAnalyzer.Analyze(lineOfText, ref currentState);
                }

                Console.WriteLine("Lexer: Lexical analyzer was successfully completed");

                _analysisInformation = _lexicalAnalyzer.AnalysisInformation;

                _syntaxAnalyzer = new SyntaxAnalyzer(_analysisInformation, new BracketsProcessor());

                _syntaxAnalyzer.ParseProgram();

                Console.WriteLine("Parser: syntactic analyzer was successfully completed");
                Console.WriteLine("RPN translation was successfully completed");

                _postfixCode = _syntaxAnalyzer.PostfixCode;
            }
            catch (Exception e)
            {
                HandleErrors(e.Message);
                return false;
            }

            return true;
        }

        private void HandleErrors(string errorMessage)
        {
            if (errorMessage.Contains("101") || errorMessage.Contains("102") || errorMessage.Contains("103") || errorMessage.Contains("104"))
            {
                var message = errorMessage;
                var errorCode = message.Substring(0, 4);
                var mainMessage = message.Substring(2);
                Console.WriteLine(mainMessage);
                Console.WriteLine($"Lexer: Analysis failed with status {errorCode}");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }

        private bool Interpret()
        {
            var result = false;
            var postfixSize = _postfixCode.Count;

            while (_iterator < postfixSize)
            {
                var tokenInfo = _postfixCode[_iterator];

                result = tokenInfo.Lexeme switch
                {
                    "boolval" or "int" or "float" or "exp" => InterpretIdentExpression(tokenInfo),
                    "input" => InterpretInputExpression(tokenInfo),
                    "print" => InterpretOutputExpression(tokenInfo),
                    "DO" => InterpretDoWhileExpression(tokenInfo),
                    "IF" => InterpretIfExpression(tokenInfo),
                    _ => InterpretSection(tokenInfo)
                };

                _iterator++;
            }

            return result;
        }

        private bool InterpretIdentExpression(TokenInformation tokenInfo)
        {
            _stack.Push(tokenInfo);
            return true;
        }

        private bool InterpretSection(TokenInformation operation)
        {
            var right = _stack.Pop();
            var left = _stack.Pop();
            var result = false;

            if (operation.Lexeme is "=" && operation.Token is "assign_op")
            {
                result = InterpretAssignment(left, right);
            }
            else if (new[] { "add_op", "mult_op", "pow_op" }.Contains(operation.Token))
            {
                result = InterpretArithmOperations(left, operation, right);
            }
            else if (operation.Token is "rel_op")
            {
                result = InterpretBooleanOperations(left, operation, right);
            }

            return result;
        }

        private bool InterpretAssignment(TokenInformation left, TokenInformation right)
        {
            if (_analysisInformation!.Ids.TryGetValue(left.Lexeme, out var leftValue))
            {
                if (leftValue.Type is null)
                {
                    throw new InterpreterException($"Variable {left.Lexeme} was not declared");
                }
            }

            if (_analysisInformation.Ids.TryGetValue(right.Lexeme, out var rightValue))
            {
                if (rightValue.Type is null)
                {
                    throw new InterpreterException($"Variable {right.Lexeme} was not declared");
                }
            }

            if (leftValue is not null && rightValue is not null)
            {
                _analysisInformation.Ids[left.Lexeme] =
                    leftValue with { Type = rightValue.Type, Value = rightValue.Value };
                return true;
            }

            return false;
        }

        private bool InterpretArithmOperations(TokenInformation left, TokenInformation operation, TokenInformation right)
        {
            var result = false;

            var leftValue = GetTokenValue(left);
            var rightValue = GetTokenValue(right);

            if (leftValue.Value is null || rightValue.Value is null)
            {
                throw new InterpreterException("Using uninitialized variables in arithmetic expression isn't allowed");
            }

            return CalculateArithmOperationValue(leftValue, operation, rightValue);
        }

        private ValueContainer GetTokenValue(TokenInformation left)
        {
            int idInTable;
            object? value = null;
            string? type = null;

            if (left.Token is "ident")
            {
                var valueInformation =
                    _analysisInformation!.Ids[left.Lexeme];

                type = valueInformation.Type ?? throw new InterpreterException($"Variable {left.Lexeme} wasn't initialized");
                value = valueInformation.Value;
                idInTable = valueInformation.IdInTable;
            }
            else
            {
                var valueInformation =
                    _analysisInformation!.Constants[left.Lexeme];
                
                idInTable = valueInformation.IdInTable;
                value = valueInformation.Value;
            }

            return new ValueContainer(idInTable, type, value);
        }

        private bool CalculateArithmOperationValue(ValueContainer left, TokenInformation operation,
            ValueContainer right)
        {
            var leftConvertedValue = ConvertToType(left);
            var rightConvertedValue = ConvertToType(right);
            dynamic value = null;

            if (operation.Lexeme is "+")
            {
                value = leftConvertedValue.Item1 + rightConvertedValue.Item1;

                if (leftConvertedValue.Item2 is not "int" && leftConvertedValue.Item2 == rightConvertedValue.Item2)
                {
                    left = left with { Type = "float" };
                }
            }
            else if (operation.Lexeme is "-")
            {
                value = leftConvertedValue.Item1 - rightConvertedValue.Item1;

                if (leftConvertedValue.Item2 is not "int" && leftConvertedValue.Item2 == rightConvertedValue.Item2)
                {
                    left = left with { Type = "float" };
                }
            }
            else if (operation.Lexeme is "*")
            {
                value = leftConvertedValue.Item1 * rightConvertedValue.Item1;

                if (leftConvertedValue.Item2 is not "int" && leftConvertedValue.Item2 == rightConvertedValue.Item2)
                {
                    left = left with { Type = "float" };
                }
            }
            else if (operation.Lexeme is "/")
            {
                if (rightConvertedValue.Item1 == 0)
                {
                    throw new InterpreterException("Division by zero");
                }
                
                value = leftConvertedValue.Item1 / rightConvertedValue.Item1;

                if (leftConvertedValue.Item2 is not "int" && leftConvertedValue.Item2 == rightConvertedValue.Item2)
                {
                    left = left with { Type = "float" };
                }
            }
            else if (operation.Lexeme is "^")
            {
                value = leftConvertedValue.Item1 ^ rightConvertedValue.Item1;

                if (leftConvertedValue.Item2 is not "int" && leftConvertedValue.Item2 == rightConvertedValue.Item2)
                {
                    left = left with { Type = "float" };
                }
            }

            var newConstLexeme = value!.ToString();

            if (!_analysisInformation!.Constants.ContainsKey(newConstLexeme))
            {
                _analysisInformation!.Constants[newConstLexeme]
                    = new ValueContainer((int) _analysisInformation!.Constants.Count + 1, left.Type, value);
            }

            _stack.Push(new TokenInformation(newConstLexeme, left.Type));

            return true;
        }

        private (dynamic?, string) ConvertToType(object? value)
        {
            dynamic? convertedValue = value as int?;
            var type = string.Empty;

            if (convertedValue is null)
            {
                convertedValue = value as float?;
                type = "float";
            }

            return (convertedValue, type);
        }

        private void InterpretBooleanOperations(TokenInformation left, TokenInformation operation, TokenInformation right)
        {
            var result = false;

            var leftValue = GetTokenValue(left);
            var rightValue = GetTokenValue(right);

            if (leftValue.Value is null || rightValue.Value is null)
            {
                throw new InterpreterException("Using uninitialized variables in arithmetic expression isn't allowed");
            }

            ProcessBooleanExpression(leftValue, operation, rightValue);
        }

        private void ProcessBooleanExpression(ValueContainer leftValue, TokenInformation operation, ValueContainer rightValue)
        {
            if (leftValue.Type is "boolval" && leftValue.Type == rightValue.Type)
            {
                leftValue = new ValueContainer(leftValue.IdInTable, "int", (int) leftValue.Value!);
                rightValue = new ValueContainer(leftValue.IdInTable, "int", (int) rightValue.Value!);
                CompareNumbers(leftValue, operation, rightValue);
                return;
            }
            else if ((new [] {"int", "float", "exp"}).Contains(leftValue.Type) && leftValue.Type == rightValue.Type)
            {
                CompareNumbers(leftValue, operation, rightValue);
                return;
            }

            throw new InterpreterException("Allowed to compare only boolean or number expression");
        }

        private void CompareNumbers(ValueContainer leftValue, TokenInformation operation, ValueContainer rightValue)
        {
            var leftFloatValue = (float) leftValue.Value!;
            var rightFloatValue = (float) leftValue.Value!;

            var result = operation.Lexeme switch
            {
                ">=" => leftFloatValue >= rightFloatValue,
                "<=" => leftFloatValue <= rightFloatValue,
                ">" => leftFloatValue > rightFloatValue,
                "<" => leftFloatValue < rightFloatValue,
                "!=" => Math.Abs(leftFloatValue - rightFloatValue) > 0,
                "==" => Math.Abs(leftFloatValue - rightFloatValue) == 0,
                _ => throw new InterpreterException("Not supported comparison operator")
            };

            var newConstLexeme = result.ToString();

            if (!_analysisInformation!.Constants.ContainsKey(newConstLexeme))
            {
                _analysisInformation!.Constants[newConstLexeme]
                    = new ValueContainer((int)_analysisInformation!.Constants.Count + 1, "boolean", result);
            }

            _stack.Push(new TokenInformation(newConstLexeme, "boolean"));

        }

        private bool InterpretInputExpression(TokenInformation tokenInfo)
        {
            throw new NotImplementedException();
        }

        private bool InterpretOutputExpression(TokenInformation tokenInfo)
        {
            throw new NotImplementedException();
        }

        private bool InterpretIfExpression(TokenInformation tokenInfo)
        {
            throw new NotImplementedException();
        }

        private bool InterpretDoWhileExpression(TokenInformation tokenInfo)
        {
            throw new NotImplementedException();
        }
    }
}
