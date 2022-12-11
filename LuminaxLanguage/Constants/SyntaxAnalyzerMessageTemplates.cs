namespace LuminaxLanguage.Constants;

public static class SyntaxAnalyzerMessageTemplates
{
    public static string ParseErrorWithUnexpectedElements =
        "Parser Error:\n\tLine {0} has unexpected elements - ('{1}' '{2}'). Expected - ('{3}' '{4}')";

    public static string ParseErrorWithUnexpectedToken =
        "Parser Error:\n\tLine {0} has unexpected token - '{1}'. Expected - '{2}'";

    public static string ParseInformation =
        "ParseToken: in row {0} lexeme - '{1}'|token - '{2}'";
}