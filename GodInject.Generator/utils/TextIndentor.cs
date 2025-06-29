namespace GodInject.Prebuild.utils
{
    public static class TextIndentor
    {
        private static readonly string[] EOLCharacters = { "\r\n", "\r", "\n" };
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
