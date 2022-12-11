namespace LuminaxLanguage.Constants;

public static class ParserMessages
{
    public static string ErrorWithUnexpectedElements =
        "Parser Error:\n\tLine {0} has unexpected elements - ('{1}' '{2}'). Expected - ('{3}' '{4}')";

    public static string ErrorWithUnexpectedToken =
        "Parser Error:\n\tLine {0} has unexpected token - '{1}'. Expected - '{2}'";

    public static string ErrorExpectedAssignToken =
        "Parser Error:\n\tLine {0} expected assign statement";

    public static string Information =
        "ParseToken: in row {0} lexeme - '{1}'|token - '{2}'";
}