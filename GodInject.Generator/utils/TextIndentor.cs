namespace GodInject.Generator.utils
{
    /// <summary>
    /// Indents the text in the C# way
    /// </summary>
    public static class TextIndentor
    {
        /// <summary>
        /// New line / ENd Of Line characters
        /// </summary>
        private static readonly string[] EOLCharacters = { "\r\n", "\r", "\n" };

        /// <summary>
        /// Indents the provided text to be like C# .cs files. Increases indentation with } decreases with {
        /// </summary>
        /// <param name="text">Text to indent</param>
        /// <returns>Indented C# text</returns>
        public static string IndentTextLikeCSharp(string text)
        {
            string[] lines = text.Split(EOLCharacters, StringSplitOptions.None);
            string newText = "";
            int currentIntentation = 0;
            foreach (string line in lines)
            {
                if (line.Trim().EndsWith('}'))
                    currentIntentation--;
                
                string indentation = new('\t', currentIntentation);
                newText += indentation + line + "\n";
                
                if (line.Trim().EndsWith('{'))
                    currentIntentation++;
            }
            return newText;
        }
    }
}
