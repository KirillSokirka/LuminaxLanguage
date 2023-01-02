namespace LuminaxLanguage.Processors
{
    public static class TypeConverter
    {
        public static Type ConvertType(string type) => type switch
        {
            "int" => typeof(int),
            "float" => typeof(float),
            "exp" => typeof(float),
            "boolean" => typeof(bool),
            "boolval" => typeof(bool),
            _ => throw new Exception("Unsupported type")
        };

        public static string ConvertType(Type type)
        {
            if (type == typeof(int))
            {
                return "int";
            }

            if (type == typeof(float))
            {
                return "float";
            }

            if (type == typeof(bool))
            {
                return "bool";
            }

            throw new Exception("Unsupported type");
        }
    }
}
