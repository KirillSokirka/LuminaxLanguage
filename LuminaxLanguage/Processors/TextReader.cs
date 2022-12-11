namespace LuminaxLanguage.Processors
{
    public static class TextReader
    {
        public static IEnumerable<string> GetLineOfText(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("This file doesn't exist");
            }

            foreach (var line in File.ReadLines(filePath))
            {
                yield return line;
            }
        }
    }
}
