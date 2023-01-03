using System.Globalization;
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
            TranslateCode(filePath);

            try
            {
                Interpret();

                Console.WriteLine("Interpreter: program has finished successfully");
            }
            catch (InterpreterException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                HandleErrors(e.Message);
            }
        }

        private void TranslateCode(string filePath)
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

            _postfixCode = _syntaxAnalyzer.PostfixCode;

            Console.WriteLine("Parser: syntactic analyzer was successfully completed");
            Console.WriteLine("RPN translation was successfully completed");
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

        private void Interpret()
        {
            var postfixSize = _postfixCode.Count;

            while (_iterator < postfixSize)
            {
                var tokenInfo = _postfixCode[_iterator];

                var result = tokenInfo switch
                {
                    { Token: "boolval" or "int" or "float" or "exp" or "ident" } => InterpretIdentExpression(tokenInfo),
                    { Lexeme: "input" } => InterpretInputExpression(),
                    { Lexeme: "print" } => InterpretOutputExpression(),
                    { Lexeme: "DO" } => InterpretDoWhileExpression(),
                    { Lexeme: "IF" } => InterpretIfExpression(),
                    _ => InterpretSection(tokenInfo)
                };

                _iterator++;
            }
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
            else if (_analysisInformation.Constants.TryGetValue(right.Lexeme, out rightValue))
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
            object? value;
            Type? type;

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
                type = typeof(float);
            }

            return new ValueContainer(idInTable, type, value);
        }

        private bool CalculateArithmOperationValue(ValueContainer left, TokenInformation operation,
            ValueContainer right)
        {
            var leftConvertedValue = ConvertToType(left.Value!.ToString());
            var rightConvertedValue = ConvertToType(right.Value!.ToString());

            var value = operation.Lexeme switch
            {
                "+" => leftConvertedValue.Item1 + rightConvertedValue.Item1,
                "-" => leftConvertedValue.Item1 - rightConvertedValue.Item1,
                "*" => leftConvertedValue.Item1 * rightConvertedValue.Item1,
                "/" => HandleDivisionByZero(rightConvertedValue, leftConvertedValue),
                "^" => (float)Math.Pow(leftConvertedValue.Item1, rightConvertedValue.Item1),
                _ => throw new InterpreterException($"This kind of operation {operation.Lexeme} is not supported")
            };

            left = HandleVariableType(left, leftConvertedValue, rightConvertedValue);

            var newConstLexeme = value.ToString(CultureInfo.CurrentCulture);

            if (!_analysisInformation!.Constants.ContainsKey(newConstLexeme))
            {
                _analysisInformation!.Constants[newConstLexeme]
                    = new ValueContainer(_analysisInformation!.Constants.Count + 1, left.Type, value);
            }

            _stack.Push(new TokenInformation(newConstLexeme, TypeConverter.ConvertType(left.Type!)));

            return true;
        }

        private static float HandleDivisionByZero((dynamic?, Type) rightConvertedValue, (dynamic?, Type) leftConvertedValue)
        {
            if (rightConvertedValue.Item1 == 0)
            {
                throw new InterpreterException("Division by zero");
            }

            float value = leftConvertedValue.Item1 / rightConvertedValue.Item1;

            return value;
        }

        private static ValueContainer HandleVariableType(ValueContainer left, (object?, Type) leftConvertedValue,
            (object?, Type) rightConvertedValue)
        {
            if (leftConvertedValue.Item2 != typeof(int) || rightConvertedValue.Item2 != typeof(int))
            {
                left = left with { Type = typeof(float) };
            }

            return left;
        }

        private (float, Type) ConvertToType(string? value)
        {
            if (int.TryParse(value, out var resultInt))
            {
                return (resultInt, typeof(int));
            }

            if (float.TryParse(value, out var resultFloat))
            {
                return (resultFloat, typeof(float));
            }

            throw new Exception("Input string is not a valid int or float value");
        }

        private bool InterpretBooleanOperations(TokenInformation left, TokenInformation operation, TokenInformation right)
        {
            var result = false;

            var leftValue = GetTokenValue(left);
            var rightValue = GetTokenValue(right);

            if (leftValue.Value is null || rightValue.Value is null)
            {
                throw new InterpreterException("Using uninitialized variables in arithmetic expression isn't allowed");
            }

            result = ProcessBooleanExpression(leftValue, operation, rightValue);

            return result;
        }

        private bool ProcessBooleanExpression(ValueContainer leftValue, TokenInformation operation, ValueContainer rightValue)
        {
            if (leftValue.Type == typeof(bool) && leftValue.Type == rightValue.Type)
            {
                leftValue = new ValueContainer(leftValue.IdInTable, typeof(int), (int)leftValue.Value!);
                rightValue = new ValueContainer(leftValue.IdInTable, typeof(int), (int)rightValue.Value!);
                CompareNumbers(leftValue, operation, rightValue);
                return true;
            }

            if (new[] { typeof(float), typeof(int) }.Contains(leftValue.Type) && leftValue.Type == rightValue.Type)
            {
                CompareNumbers(leftValue, operation, rightValue);
                return true;
            }

            throw new InterpreterException("Allowed to compare only boolean or number expression");
        }

        private void CompareNumbers(ValueContainer leftValue, TokenInformation operation, ValueContainer rightValue)
        {
            var leftFloatValue = float.Parse(leftValue.Value!.ToString()!);
            var rightFloatValue = float.Parse(rightValue.Value!.ToString()!);

            var result = operation.Lexeme.Trim() switch
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
                    = new ValueContainer(_analysisInformation!.Constants.Count + 1, typeof(bool), result);
            }

            var temp = _stack.Pop();
            _stack.Push(new TokenInformation(newConstLexeme, "boolean"));
            _stack.Push(temp);
        }

        private bool InterpretInputExpression()
        {
            var tokenInfo = _stack.Pop();

            var tokenValue = GetTokenValue(tokenInfo);

            if (tokenValue.Type is null)
            {
                throw new InterpreterException($"There is no declared variable with name {tokenInfo.Lexeme}");
            }

            var data = Console.ReadLine();

            tokenValue = ParseConsoleInput(data, tokenValue);

            if (tokenValue is not null)
            {
                _analysisInformation!.Ids[tokenInfo.Lexeme] = tokenValue;
                return true;
            }

            return false;
        }

        private ValueContainer? ParseConsoleInput(string? data, ValueContainer valueContainer)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new InterpreterException("No data was provided for variable input");
            }

            var newValueContainer = valueContainer;

            if (int.TryParse(data, out var value) && newValueContainer.Type == typeof(int))
            {
                newValueContainer = newValueContainer with { Value = value, Type = typeof(int) };
            }
            else if (float.TryParse(data, out var floatValue) && newValueContainer.Type == typeof(float))
            {
                newValueContainer = newValueContainer with { Value = floatValue, Type = typeof(float) };
            }
            else if (bool.TryParse(data, out var boolValue) && newValueContainer.Type == typeof(bool))
            {
                newValueContainer = newValueContainer with { Value = boolValue, Type = typeof(bool) };
            }
            else
            {
                throw new InterpreterException("Invalid input");
            }

            return newValueContainer;
        }

        private bool InterpretOutputExpression()
        {
            var tokenInfo = _stack.Pop();

            var tokenValue = GetTokenValue(tokenInfo);

            if (tokenValue.Type is null)
            {
                throw new InterpreterException($"The variable {tokenInfo.Lexeme} wasn't declared");
            }

            if (tokenValue.Value is null)
            {
                throw new InterpreterException($"The variable {tokenInfo.Lexeme} wasn't initialized");
            }

            Console.WriteLine($"The variable - {tokenInfo.Lexeme} has value - {tokenValue.Value} ({tokenValue.Type})");

            return true;
        }

        private bool InterpretIfExpression()
        {
            throw new NotImplementedException();
        }

        private bool InterpretDoWhileExpression()
        {
            throw new NotImplementedException();
        }
    }
}
