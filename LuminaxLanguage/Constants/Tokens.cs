namespace LuminaxLanguage.Constants
{
    public class Tokens
    {
        public static Dictionary<string, string> LanguageTokens = new()
        {
            {"program", "keyword"},
            {"int", "keyword"},
            {"float", "keyword"},
            {"boolean", "keyword"},
            {"while", "keyword"},
            {"do", "keyword"},
            {"if", "keyword"},
            {"then", "keyword"},
            {"input", "keyword"},
            {"=", "assign_op"},
            {"<=", "rel_op"},
            {">=", "rel_op"},
            {"<", "rel_op"},
            {">", "rel_op"},
            {"==", "rel_op"},
            {"!=", "rel_op"},
            {".", "punct"},
            {",", "punct"},
            {":", "punct"},
            {";", "punct"},
            {"E", "punct"},
            {" ", "ws"},
            {"\t", "ws"},
            {"\n", "nl"},
            {"\r\n", "nl"},
            {"-", "add_op"},
            {"+", "add_op"},
            {"*", "mult_op"},
            {"/", "mult_op"},
            {"^", "power_op"},
            {"(", "par_op"},
            {")", "par_op"},
            {"{", "par_op"},
            {"}", "par_op"},
            {"true", "boolval"},
            {"false", "boolval"}
        };

        public static Dictionary<int, string> OtherTokens = new()
        {
            { 3, "ident" },
            { 12, "int" },
            { 16, "float" },
            { 18, "exp"} 
        };
    }
}
